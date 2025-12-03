# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemZarzadzaniaBibliotekaFirmy.csproj", "."]
RUN dotnet restore "./SystemZarzadzaniaBibliotekaFirmy.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SystemZarzadzaniaBibliotekaFirmy.csproj" -c Release -o /app/build
RUN dotnet publish "SystemZarzadzaniaBibliotekaFirmy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SystemZarzadzaniaBibliotekaFirmy.dll"]
