#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Alpha/Alpha.csproj", "Alpha/"]
RUN dotnet restore "Alpha/Alpha.csproj"
COPY . .
WORKDIR "/src/Alpha"
RUN dotnet build "Alpha.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alpha.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5670/udp
EXPOSE 5670-5671/tcp
ENTRYPOINT ["dotnet", "Alpha.dll"]