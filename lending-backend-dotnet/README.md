# Lending Backend .NET

這是原本 Go `lending-backend` 的 C# / ASP.NET Core 10 版本，採用 DDD 與 Clean Architecture 分層：

- `LendingSystem.Domain`: domain entities 與 domain constants。
- `LendingSystem.Application`: use cases、DTO、repository/storage/token 抽象。
- `LendingSystem.Infrastructure`: PostgreSQL、MinIO、JWT、BCrypt、gRPC 實作。
- `LendingSystem.WebApi`: ASP.NET Core controllers、middleware、OpenAPI。

## Run

```bash
dotnet restore
dotnet run --project src/LendingSystem.WebApi
```

預設會 listen `http://0.0.0.0:8000`，可用原 Go backend 的環境變數：

- `APP_PORT`
- `DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`, `DB_NAME`
- `MINIO_ENDPOINT`, `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY`, `MINIO_BUCKET_NAME`
- `SECRET_KEY`
- `VIDEO_SERVICE_ADDR`
