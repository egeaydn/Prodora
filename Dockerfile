# Build: docker build -t prodora .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, with only the project files, so this layer is reused until a package changes.
COPY Prodora.Entitys/Prodora.Entitys.csproj Prodora.Entitys/
COPY Prodora.DataAccess/Prodora.DataAccess.csproj Prodora.DataAccess/
COPY Prodora.Business/Prodora.Business.csproj Prodora.Business/
COPY Prodora.WebUI/Prodora.WebUI.csproj Prodora.WebUI/
RUN dotnet restore Prodora.WebUI/Prodora.WebUI.csproj

COPY Prodora.Entitys/ Prodora.Entitys/
COPY Prodora.DataAccess/ Prodora.DataAccess/
COPY Prodora.Business/ Prodora.Business/
COPY Prodora.WebUI/ Prodora.WebUI/
RUN dotnet publish Prodora.WebUI/Prodora.WebUI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# The image ships a non-root user; nothing here needs to write to disk (logs go to stdout).
USER $APP_UID
ENTRYPOINT ["dotnet", "Prodora.WebUI.dll"]
