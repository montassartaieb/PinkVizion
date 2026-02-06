FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Gateway/PinkVision.Gateway.csproj", "PinkVision.Gateway/"]
RUN dotnet restore "PinkVision.Gateway/PinkVision.Gateway.csproj"
COPY . .
WORKDIR "/src/PinkVision.Gateway"
RUN dotnet build "PinkVision.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PinkVision.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PinkVision.Gateway.dll"]
