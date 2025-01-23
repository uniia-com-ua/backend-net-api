# Use the official .NET image as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UNIIAadminAPI/UNIIAadminAPI.csproj", "UNIIAadminAPI/"]
RUN dotnet restore "UNIIAadminAPI/UNIIAadminAPI.csproj"
COPY . .
WORKDIR "/src/UNIIAadminAPI"
RUN dotnet build "UNIIAadminAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UNIIAadminAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UNIIAadminAPI.dll"]