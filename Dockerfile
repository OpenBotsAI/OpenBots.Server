#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:12.7-alpine As NpmBuild
WORKDIR /src
COPY ["OpenBots.Server.Web/OpenBots.Server.Web.csproj", "OpenBots.Server.Web/"]
COPY ["OpenBots.Server.Model/OpenBots.Server.Model.csproj", "OpenBots.Server.Model/"]
COPY ["OpenBots.Server.Business/OpenBots.Server.Business.csproj", "OpenBots.Server.Business/"]
COPY ["OpenBots.Server.DataAccess/OpenBots.Server.DataAccess.csproj", "OpenBots.Server.DataAccess/"]
COPY ["OpenBots.Server.Infrastructure/OpenBots.Server.Infrastructure.csproj", "OpenBots.Server.Infrastructure/"]
COPY ["OpenBots.Server.Core/OpenBots.Server.Core.csproj", "OpenBots.Server.Core/"]
COPY ["OpenBots.Server.ViewModel/OpenBots.Server.ViewModel.csproj", "OpenBots.Server.ViewModel/"]
COPY ["OpenBots.Server.Security/OpenBots.Server.Security.csproj", "OpenBots.Server.Security/"]
COPY ["OpenBots.Server.Infrastructure.Azure/OpenBots.Server.Infrastructure.Azure.csproj", "OpenBots.Server.Infrastructure.Azure/"]
COPY . .

WORKDIR "/src/OpenBots.Server.Web/ClientApp"
RUN npm install angular
RUN npm install -g ng-cli
RUN npm install
RUN npm run build --prod

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
WORKDIR "/src/OpenBots.Server.Web"
RUN dotnet restore "OpenBots.Server.Web.csproj"
RUN dotnet build "OpenBots.Server.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenBots.Server.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenBots.Server.Web.dll"]