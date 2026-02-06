FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PinkVision.Appointment.API/PinkVision.Appointment.API.csproj", "PinkVision.Appointment.API/"]
RUN dotnet restore "PinkVision.Appointment.API/PinkVision.Appointment.API.csproj"
COPY . .
WORKDIR "/src/PinkVision.Appointment.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PinkVision.Appointment.API.dll"]
