FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Patient.API/PinkVision.Patient.API.csproj", "PinkVision.Patient.API/"]
RUN dotnet restore "PinkVision.Patient.API/PinkVision.Patient.API.csproj"
COPY . .
WORKDIR "/src/PinkVision.Patient.API"
RUN dotnet build "PinkVision.Patient.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PinkVision.Patient.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PinkVision.Patient.API.dll"]
