FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

RUN apt-get update && apt-get install -y \
    iputils-ping \
    traceroute \
    net-tools \
    curl \
    wget \
    dnsutils \
    iproute2 \
    python3-full \
    pip \
    libleptonica-dev \
    libc6-dev \
    tesseract-ocr \
    libtesseract-dev \
    tesseract-ocr-eng \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY . /app/
EXPOSE 8000
ENTRYPOINT ["dotnet", "DocumentAtom.Server.dll"]
