FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch-arm32v7 AS build
WORKDIR /app

# publish app
COPY . .
WORKDIR /app/samples
RUN dotnet restore
RUN dotnet publish -c release -r linux-arm -o out

## run app
FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim-arm32v7 AS runtime
WORKDIR /app
COPY --from=build /app/samples/out ./

# install System.Drawing native dependencies
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libc6-dev libgdiplus libx11-dev && \
    rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "VideoDevice.Samples.dll"]
