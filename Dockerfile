# GIAI ĐOẠN 1: Build (Sử dụng .NET SDK để build code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép file .csproj và phục hồi (restore) các gói nuget
# SỬA Ở ĐÂY: Chỉ định đường dẫn chính xác đến file .csproj
COPY BackendEcommerce/BackendEcommerce.csproj BackendEcommerce/
RUN dotnet restore BackendEcommerce/BackendEcommerce.csproj

# Sao chép toàn bộ source code còn lại và build
# SỬA Ở ĐÂY: Chỉ định thư mục code
COPY BackendEcommerce/ BackendEcommerce/
RUN dotnet build BackendEcommerce/BackendEcommerce.csproj -c Release -o /app/build

# GIAI ĐOẠN 2: Publish (Tạo ra các file chạy thực thi)
FROM build AS publish
# SỬA Ở ĐÂY: Chỉ định project để publish
RUN dotnet publish BackendEcommerce/BackendEcommerce.csproj -c Release -o /app/publish /p:UseAppHost=false

# GIAI ĐOẠN 3: Final (Image cuối cùng siêu nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Mặc định, .NET 8 chạy trên port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# SỬA Ở ĐÂY: Chỉ định đúng tên .dll
ENTRYPOINT ["dotnet", "BackendEcommerce.dll"]

