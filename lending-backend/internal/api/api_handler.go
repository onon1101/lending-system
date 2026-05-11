package api

import (
	"context"
	"errors"
	"io"
	"log"
	"net/http"
	"net/url"
	"object-borrow-system/internal/model"
	pb "object-borrow-system/internal/video_service"
	"os"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

const defaultVideoServiceAddress = "192.168.2.236:50051"

type APIHandler struct {
	DBClient model.DBClient
}

func NewAPIHandler(db model.DBClient) *APIHandler {
	return &APIHandler{
		DBClient: db,
	}
}

type serviceHealthResponse struct {
	Status    string `json:"status"`
	Service   string `json:"service"`
	Timestamp string `json:"timestamp"`
}

type systemStatusResponse struct {
	Status    string           `json:"status"`
	Service   string           `json:"service"`
	Database  dependencyStatus `json:"database"`
	Timestamp string           `json:"timestamp"`
}

type dependencyStatus struct {
	Status string `json:"status"`
	Error  string `json:"error,omitempty"`
}

type errorResponse struct {
	Error string `json:"error"`
}

// @Summary 服務與資料庫健康檢查
// @Description 檢查 API 服務是否運行，以及 PostgreSQL 資料庫連線是否成功。
// @Tags System
// @Produce json
// @Success 200 {object} systemStatusResponse "服務與資料庫正常"
// @Failure 503 {object} systemStatusResponse "服務正常但資料庫異常"
// @Router /api/status [get]
func (h *APIHandler) GetSystemStatus(c *gin.Context) {
	response := systemStatusResponse{
		Status:    "ok",
		Service:   "ok",
		Database:  dependencyStatus{Status: "ok"},
		Timestamp: time.Now().UTC().Format(time.RFC3339),
	}

	statusCode := http.StatusOK

	if h.DBClient == nil {
		response.Status = "degraded"
		response.Database = dependencyStatus{
			Status: "error",
			Error:  "database client is not configured",
		}
		statusCode = http.StatusServiceUnavailable
	} else if err := h.DBClient.Ping(); err != nil {
		response.Status = "degraded"
		response.Database = dependencyStatus{
			Status: "error",
			Error:  err.Error(),
		}
		statusCode = http.StatusServiceUnavailable
	}

	c.JSON(statusCode, response)
}

// @Summary 健康檢查
// @Description 檢查服務是否正常運行
// @Tags System
// @Produce json
// @Success 200 {object} serviceHealthResponse "服務正常"
// @Router /api/health [get]
func (h *APIHandler) HealthCheck(c *gin.Context) {
	c.JSON(http.StatusOK, serviceHealthResponse{
		Status:    "ok",
		Service:   "lending-backend",
		Timestamp: time.Now().UTC().Format(time.RFC3339),
	})
}

// @Summary 測試下載並串流影音媒體
// @Description 透過 gRPC 連線至影片服務，根據提供的 URL 下載影片並以 HTTP Chunked 方式即時串流回傳給客戶端。
// @Tags System
// @Produce application/octet-stream
// @Param url query string true "影片原始來源網址"
// @Success 200 {file} binary "影音檔案流 (MP4)"
// @Failure 400 {object} errorResponse "缺少或無效的 URL 參數"
// @Failure 502 {object} errorResponse "影片服務回傳錯誤"
// @Failure 504 {object} errorResponse "影片服務連線逾時"
// @Router /api/download [get]
func (h *APIHandler) TestDownloadMedia(c *gin.Context) {
	videoURL := strings.TrimSpace(c.Query("url"))
	if videoURL == "" {
		c.JSON(http.StatusBadRequest, errorResponse{Error: "url query parameter is required"})
		return
	}

	parsedURL, err := url.ParseRequestURI(videoURL)
	if err != nil || parsedURL.Scheme == "" || parsedURL.Host == "" {
		c.JSON(http.StatusBadRequest, errorResponse{Error: "url query parameter must be a valid absolute URL"})
		return
	}

	conn, err := dialVideoService(c.Request.Context(), videoServiceAddress())
	if err != nil {
		status := http.StatusBadGateway
		if errors.Is(err, context.DeadlineExceeded) {
			status = http.StatusGatewayTimeout
		}
		c.JSON(status, errorResponse{Error: "failed to connect to video service"})
		return
	}
	defer conn.Close()

	client := pb.NewVideoServiceClient(conn)
	stream, err := client.DownloadAndStream(c.Request.Context(), &pb.DownloadRequest{Url: videoURL})
	if err != nil {
		log.Printf("failed to start video download stream: %v", err)
		c.JSON(http.StatusBadGateway, errorResponse{Error: "failed to start video download stream"})
		return
	}

	c.Header("Content-Type", "application/octet-stream")
	c.Header("Content-Disposition", `attachment; filename="downloaded_video.mp4"`)
	c.Header("Transfer-Encoding", "chunked")

	wroteChunk := false
	for {
		resp, err := stream.Recv()
		if err == io.EOF {
			return
		}
		if err != nil {
			log.Printf("video download stream failed: %v", err)
			if !wroteChunk {
				c.JSON(http.StatusBadGateway, errorResponse{Error: "video download stream failed"})
			}
			return
		}

		if resp.GetFilename() != "" && !wroteChunk {
			c.Header("Content-Disposition", `attachment; filename="`+sanitizeHeaderFilename(resp.GetFilename())+`"`)
		}

		switch payload := resp.GetPayload().(type) {
		case *pb.DownloadResponse_FileChunk:
			if len(payload.FileChunk) == 0 {
				continue
			}
			if _, err := c.Writer.Write(payload.FileChunk); err != nil {
				log.Printf("failed to write video chunk to response: %v", err)
				return
			}
			wroteChunk = true
			c.Writer.Flush()
		case *pb.DownloadResponse_Progress:
			log.Printf("video download progress filename=%q progress=%d", resp.GetFilename(), payload.Progress)
		case *pb.DownloadResponse_ErrorMes:
			log.Printf("video service returned error: %s", payload.ErrorMes)
			if !wroteChunk {
				c.JSON(http.StatusBadGateway, errorResponse{Error: payload.ErrorMes})
			}
			return
		default:
			log.Printf("video service returned an empty payload")
		}
	}
}

func dialVideoService(parent context.Context, address string) (*grpc.ClientConn, error) {
	ctx, cancel := context.WithTimeout(parent, 5*time.Second)
	defer cancel()

	return grpc.DialContext(
		ctx,
		address,
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithBlock(),
	)
}

func videoServiceAddress() string {
	if address := strings.TrimSpace(os.Getenv("VIDEO_SERVICE_ADDR")); address != "" {
		return address
	}
	return defaultVideoServiceAddress
}

func sanitizeHeaderFilename(filename string) string {
	filename = strings.ReplaceAll(filename, "\\", "_")
	filename = strings.ReplaceAll(filename, "\"", "_")
	filename = strings.ReplaceAll(filename, "\r", "_")
	filename = strings.ReplaceAll(filename, "\n", "_")
	filename = strings.TrimSpace(filename)
	if filename == "" {
		return "downloaded_video.mp4"
	}
	return filename
}
