FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5008

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Dashboard.API/PinkVision.Dashboard.API.csproj", "PinkVision.Dashboard.API/"]
RUN dotnet restore "PinkVision.Dashboard.API/PinkVision.Dashboard.API.csproj"
COPY . .
WORKDIR "/src/PinkVision.Dashboard.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PinkVision.Dashboard.API.dll"]
