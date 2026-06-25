# Sử dụng .NET SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy file project (.csproj) và restore các dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ code còn lại và build sản phẩm
COPY . ./
RUN dotnet publish -c Release -o out

# Sử dụng mã runtime nhỏ gọn hơn để chạy sản phẩm
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Cấu hình cổng chạy mặc định cho Render (Render dùng port 8080 hoặc biến môi trường PORT)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Medicine.dll"]
