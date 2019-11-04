FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Tinamous.MeasurementsProcessor.ConsoleHost/Tinamous.MeasurementsProcessor.ConsoleHost.csproj", "Tinamous.MeasurementsProcessor.ConsoleHost/"]
RUN dotnet restore "Tinamous.MeasurementsProcessor.ConsoleHost/Tinamous.MeasurementsProcessor.ConsoleHost.csproj"  -s https://www.myget.org/F/tinamous/api/v3/index.json -s https://api.nuget.org/v3/index.json
COPY . .
WORKDIR "/src/Tinamous.MeasurementsProcessor.ConsoleHost"
RUN dotnet build "Tinamous.MeasurementsProcessor.ConsoleHost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Tinamous.MeasurementsProcessor.ConsoleHost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Tinamous.MeasurementsProcessor.ConsoleHost.dll"]