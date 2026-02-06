FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Doctor.API/PinkVision.Doctor.API.csproj", "PinkVision.Doctor.API/"]
RUN dotnet restore "PinkVision.Doctor.API/PinkVision.Doctor.API.csproj"
COPY . .
WORKDIR "/src/PinkVision.Doctor.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PinkVision.Doctor.API.dll"]
