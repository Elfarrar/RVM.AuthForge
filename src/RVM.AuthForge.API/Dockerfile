FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY nuget.config .
COPY ["src/RVM.AuthForge.API/RVM.AuthForge.API.csproj", "src/RVM.AuthForge.API/"]
COPY ["src/RVM.AuthForge.Domain/RVM.AuthForge.Domain.csproj", "src/RVM.AuthForge.Domain/"]
COPY ["src/RVM.AuthForge.Infrastructure/RVM.AuthForge.Infrastructure.csproj", "src/RVM.AuthForge.Infrastructure/"]
RUN dotnet restore "./src/RVM.AuthForge.API/RVM.AuthForge.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.AuthForge.API"
RUN dotnet build "./RVM.AuthForge.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RVM.AuthForge.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.AuthForge.API.dll"]
