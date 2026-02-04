package api

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"object-borrow-system/internal/model"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"

	pb "object-borrow-system/internal/video_service"
)

type APIHandler struct {
	DBClient model.DBClient
}

func NewAPIHandler(db model.DBClient) *APIHandler {
	return &APIHandler{
		DBClient: db,
	}
}

// @Summary 服務與資料庫健康檢查
// @Description 檢查 API 服務是否運行，以及 PostgreSQL 資料庫連線是否成功。
// @Tags System
// @Produce json
// @Success 200 {object} map[string]string "狀態: ok, DB: ok"
// @Failure 503 {object} map[string]string "狀態: ok, DB: error"
// @Router /api/status [get]
func (h *APIHandler) GetSystemStatus(w http.ResponseWriter, r *http.Request) {
	dbStatus := "ok"
	statusCode := http.StatusOK

	if err := h.DBClient.Ping(); err != nil {
		dbStatus = "error"
		statusCode = http.StatusServiceUnavailable
	}

	response := map[string]string{
		"service":  "ok",
		"database": dbStatus,
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(response)
}

// @Summary 健康檢查
// @Description 檢查服務是否正常運行
// @Tags System
// @Produce plain
// @Success 200 {string} string "Service is running!"
// @Router /api/health [get]
func (h *APIHandler) HealthCheck(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
	fmt.Fprint(w, "Service is running!")
}

// @Summary 測試下載並串流影音媒體
// @Description 透過 gRPC 連線至影片服務，根據提供的 URL 下載影片並以 HTTP Chunked 方式即時串流回傳給客戶端。
// @Tags System
// @Accept json
// @Produce application/octet-stream
// @Param url query string true "影片原始來源網址"
// @Success 200 {file} binary "影音檔案流 (MP4)"
// @Failure 400 {object} map[string]string "缺少 URL 參數"
// @Failure 500 {object} map[string]string "gRPC 連線失敗或串流錯誤"
// @Router /api/download [get]
func (h *APIHandler) TestDownloadMedia(w http.ResponseWriter, r *http.Request) {
	/*
		Todo: 這邊記得要做抽象。先把它整合進入/api/item/media api裡面，然後把檔案串入進去 minio server 當中，把 records 到 database 當中

	*/
	videoURL := r.URL.Query().Get("url")
	if videoURL == "" {
		http.Error(w, "URL is required", http.StatusBadRequest)
		return
	}

	conn, err := grpc.Dial("192.168.2.236:50051", grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		http.Error(w, "Failed to connect to video service", http.StatusInternalServerError)
		return
	}
	defer conn.Close()

	client := pb.NewVideoServiceClient(conn)
	stream, err := client.DownloadAndStream(context.Background(), &pb.DownloadRequest{Url: videoURL})
	if err != nil {
		http.Error(w, "Failed to start download stream", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Disposition", `"attachment; filename=\"downloaded_video.mp4\""`)
	w.Header().Set("Content-Type", "application/octet-stream")
	w.Header().Set("Transfer-Encoding", "chunked")

	for {
		resp, err := stream.Recv()
		if err == io.EOF {
			break
		}
		if err != nil {
			return
		}

		switch x := resp.Payload.(type) {
		case *pb.DownloadResponse_FileChunk:
			// 將檔案碎片寫入 HTTP 響應
			w.Write(x.FileChunk)
			// 強制刷新，讓前端能即時收到數據
			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			}
		case *pb.DownloadResponse_Progress:
			// 進度資訊通常記錄在 Server Log，或透過 WebSocket 傳給前端
			// 這裡不寫入 HTTP Body 以免破壞檔案二進位格式
			progress := x.Progress

			// 2. 顯示進度 Log
			// \r 會讓游標回到行首，這樣進度條就會在同一行更新，不會洗掉整個螢幕
			fmt.Printf("\r📥 正在下載 [%s] 下載進度: %d%%          ", resp.Filename, progress)
			// 如果進度達到 100%，換行以保持 Log 整齊
			if progress >= 100 {
				fmt.Println("\n✅ 下載完成，準備傳送檔案碎片...")
			}
			// _ = x.Progress
		case *pb.DownloadResponse_ErrorMes:
			http.Error(w, x.ErrorMes, http.StatusInternalServerError)
			return
		}
		if resp.Filename != "" {
			w.Header().Set("Content-Disposition", "attachment; filename=\""+resp.Filename+"\"")
		}
	}
}
