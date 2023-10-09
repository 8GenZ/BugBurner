#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Continue with the remaining build steps
COPY . .
WORKDIR "/src/BugBurner"
RUN dotnet build "BugBurner.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BugBurner.csproj", "."]
RUN dotnet restore "./BugBurner.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BugBurner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BugBurner.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BugBurner.dll"]