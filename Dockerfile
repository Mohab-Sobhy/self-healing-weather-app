# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات الحل
COPY global.json .
COPY SelfHealingWeatherApp.sln .
COPY SelfHealingWeatherApp/ SelfHealingWeatherApp/

# استرجاع الحزم
RUN dotnet restore

# بناء ونشر التطبيق
RUN dotnet publish SelfHealingWeatherApp/SelfHealingWeatherApp.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# تثبيت curl (مفيد للـ health checks و debugging)
USER root
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# إعداد المنفذ
ENV ASPNETCORE_URLS=http://+:5142

# نسخ ناتج البناء
COPY --from=build /app/publish .

# فتح المنفذ
EXPOSE 5142

# تشغيل التطبيق
ENTRYPOINT ["dotnet", "SelfHealingWeatherApp.dll"]

