FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend/SillasTresCantos.Api/SillasTresCantos.Api.csproj", "Backend/SillasTresCantos.Api/"]
RUN dotnet restore "Backend/SillasTresCantos.Api/SillasTresCantos.Api.csproj"
COPY . .
WORKDIR "/src/Backend/SillasTresCantos.Api"
RUN dotnet publish "SillasTresCantos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SillasTresCantos.Api.dll"]
