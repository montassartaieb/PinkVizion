FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Imaging.API/PinkVision.Imaging.API.csproj", "PinkVision.Imaging.API/"]
RUN dotnet restore "PinkVision.Imaging.API/PinkVision.Imaging.API.csproj"
COPY . .
WORKDIR "/src/PinkVision.Imaging.API"
RUN dotnet build "PinkVision.Imaging.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PinkVision.Imaging.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "PinkVision.Imaging.API.dll"]
