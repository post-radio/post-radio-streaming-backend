FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5000

RUN groupadd -r noncasted && useradd -r -g noncasted noncasted
RUN mkdir -p /app/audiofiles
RUN chown -R noncasted:noncasted /app/audiofiles

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "Core/Core.csproj"
COPY . .
WORKDIR "/src/Core"
RUN dotnet build "Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.dll"]
