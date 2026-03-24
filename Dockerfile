# Используем образ с SDK для сборки проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Указываем правильный путь с учетом подпапки Visual Studio
COPY ["CollegeProcurementDSS/CollegeProcurementDSS.csproj", "CollegeProcurementDSS/"]
RUN dotnet restore "CollegeProcurementDSS/CollegeProcurementDSS.csproj"

# Копируем весь остальной код проекта
COPY . .

# Переходим в подпапку с кодом и собираем проект
WORKDIR "/src/CollegeProcurementDSS"
RUN dotnet publish "CollegeProcurementDSS.csproj" -c Release -o /app/publish

# Берем легкий образ для запуска (без SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Принудительно заставляем приложение слушать 80-й порт внутри контейнера
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CollegeProcurementDSS.dll"]