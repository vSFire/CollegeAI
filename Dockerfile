# Используем образ с SDK для сборки проекта (укажи 6.0, если проект на .NET 6)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["CollegeProcurementDSS.csproj", "./"]
RUN dotnet restore "./CollegeProcurementDSS.csproj"

# Копируем весь остальной код и собираем
COPY . .
RUN dotnet publish "CollegeProcurementDSS.csproj" -c Release -o /app/publish

# Берем легкий образ для запуска (без SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Принудительно заставляем приложение слушать 80-й порт внутри контейнера
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CollegeProcurementDSS.dll"]