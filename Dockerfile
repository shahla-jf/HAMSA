# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HAMSA.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# تنظیمات کامل برای جلوگیری از مشکلات inotify
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:10000

# افزایش محدودیت‌های سیستم (اختیاری)
RUN apt-get update && apt-get install -y procps && rm -rf /var/lib/apt/lists/*
RUN echo "fs.inotify.max_user_watches=524288" >> /etc/sysctl.conf \
    && echo "fs.inotify.max_user_instances=1024" >> /etc/sysctl.conf

EXPOSE 10000
ENTRYPOINT ["dotnet", "HAMSA.dll"]