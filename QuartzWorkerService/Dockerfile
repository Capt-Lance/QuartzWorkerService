FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["QuartzWorkerService/QuartzWorkerService.csproj", "QuartzWorkerService/"]
RUN dotnet restore "QuartzWorkerService/QuartzWorkerService.csproj"
COPY . .
WORKDIR "/src/QuartzWorkerService"
RUN dotnet build "QuartzWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuartzWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuartzWorkerService.dll"]