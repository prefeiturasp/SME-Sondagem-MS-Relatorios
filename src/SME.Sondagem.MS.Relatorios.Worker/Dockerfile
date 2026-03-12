FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libx11-6 \
    libxrender1 \
    libxext6 \
    libfontconfig1 \
    libfreetype6 \
    libpng16-16 \
    libjpeg62-turbo \
    zlib1g \
    libicu-dev \
    xfonts-75dpi \
    xfonts-base \
    && rm -rf /var/lib/apt/lists/*

ENV LD_LIBRARY_PATH="/app:/usr/lib"

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/SME.Sondagem.MS.Relatorios.Worker/SME.Sondagem.MS.Relatorios.Worker.csproj", "src/SME.Sondagem.MS.Relatorios.Worker/"]
COPY ["src/SME.Sondagem.MS.Relatorios.HtmlPdf/SME.Sondagem.MS.Relatorios.HtmlPdf.csproj", "src/SME.Sondagem.MS.Relatorios.HtmlPdf/"]

RUN dotnet restore "src/SME.Sondagem.MS.Relatorios.Worker/SME.Sondagem.MS.Relatorios.Worker.csproj"

COPY . .
WORKDIR "/src/src/SME.Sondagem.MS.Relatorios.Worker"
RUN dotnet build "SME.Sondagem.MS.Relatorios.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SME.Sondagem.MS.Relatorios.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN chmod +x /app/libwkhtmltox.so
USER $APP_UID

ENTRYPOINT ["dotnet", "SME.Sondagem.MS.Relatorios.Worker.dll"]