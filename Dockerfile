# Stage 1: Base runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY nuget.config .    

# Copy project file
COPY ["Amplify.Jobs.DigitalOnboarding/Amplify.Jobs.DigitalOnboarding.csproj", "Amplify.Jobs.DigitalOnboarding/"]

# Restore packages
RUN dotnet restore "./Amplify.Jobs.DigitalOnboarding/Amplify.Jobs.DigitalOnboarding.csproj"

# Copy full source
COPY . .

# Build
WORKDIR "/src/Amplify.Jobs.DigitalOnboarding"
RUN dotnet build "./Amplify.Jobs.DigitalOnboarding.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Amplify.Jobs.DigitalOnboarding.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
