#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Use a compatible ASP.NET runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use a compatible .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BugBurner/BugBurner.csproj", "BugBurner/"]
RUN dotnet restore "BugBurner/BugBurner.csproj"
COPY . .
WORKDIR "/src/BugBurner"  # Corrected the WORKDIR path
RUN dotnet build "BugBurner.csproj" -c Release -o /app/build

# Create a publish image
FROM build AS publish
RUN dotnet publish "BugBurner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create a final image for running the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BugBurner.dll"]