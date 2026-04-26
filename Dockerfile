# 🔹 Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución y proyectos primero (mejor caching)
COPY *.sln .
COPY Dtos/*.csproj Dtos/
COPY MyApp.Api/*.csproj MyApp.Api/
COPY Repositories/*.csproj Repositories/
COPY Services/*.csproj Services/

# Restaurar dependencias
RUN dotnet restore MyApp.Api/MyApp.Api.csproj

# Copiar todo lo demás
COPY . .

# Publicar
RUN dotnet publish MyApp.Api/MyApp.Api.csproj -c Release -o /app/publish

# 🔹 Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]