# Використання офіційного .NET образу як базового
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7193

# Використання SDK образу для збірки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіювання та відновлення проектів
COPY ["UNIIAadminAPI/UniiaAdmin.WebApi.csproj", "UNIIAadminAPI/"]
COPY ["UniiaAdminAPI.Data/UniiaAdmin.Data.csproj", "UniiaAdminAPI.Data/"]
RUN dotnet restore "UNIIAadminAPI/UniiaAdmin.WebApi.csproj"

# Копіювання всього рішення та збірка
COPY . .
WORKDIR "/src/UNIIAadminAPI"
RUN dotnet build "UniiaAdmin.WebApi.csproj" -c Release -o /app/build

# Публікація проекту
FROM build AS publish
RUN dotnet publish "UniiaAdmin.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Фінальний етап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Налаштування середовища
ENV ASPNETCORE_URLS=http://+:7193

ENTRYPOINT ["dotnet", "UniiaAdmin.WebApi.dll"] 