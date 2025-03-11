FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set ARCH properly using ARG and ENV
ARG TARGETARCH
ENV ARCH=${TARGETARCH}
RUN echo "Building for TARGETARCH: ${TARGETARCH}"

# Install required packages
RUN apt-get update && apt-get install -y \
    iputils-ping \
    traceroute \
    net-tools \
    curl \
    wget \
    dnsutils \
    iproute2 \
    vim \
    file \
    python3-full \
    pip \
    libleptonica-dev \
    libc6-dev \
    tesseract-ocr \
    libtesseract-dev \
    tesseract-ocr-eng \
    && rm -rf /var/lib/apt/lists/*

# Create necessary directories for all architectures
RUN mkdir -p \
    /usr/lib/aarch64-linux-gnu \
    /usr/lib/x86_64-linux-gnu \
    /usr/lib/i386-linux-gnu \
    /runtimes/linux-arm64/native \
    /runtimes/linux-x64/native \
    /runtimes/linux-x86/native \
    /app/arm64 \
    /app/x64 \
    /app/x86

# Copy libraries with architecture-specific handling
RUN echo "Architecture: ${TARGETARCH}" && \
    # Find the libraries
    LEPT_SO=$(find /usr/lib -name "liblept.so.5*" | head -n1) && \
    TESS_SO=$(find /usr/lib -name "libtesseract.so.5*" | head -n1) && \
    if [ "${TARGETARCH}" = "arm64" ]; then \
        DL_SO=/lib/aarch64-linux-gnu/libdl.so.2; \
    else \
        DL_SO=/lib/x86_64-linux-gnu/libdl.so.2; \
    fi && \
    echo "Found leptonica at: $LEPT_SO" && \
    echo "Found tesseract at: $TESS_SO" && \
    echo "Found libdl at: $DL_SO" && \
    # Ensure files exist before copying
    if [ ! -f "$LEPT_SO" ] || [ ! -f "$TESS_SO" ] || [ ! -f "$DL_SO" ]; then \
        echo "Error: Required libraries not found" && \
        ls -la /usr/lib/*-linux-gnu/ | grep "liblept\|libtesseract" && \
        ls -la /lib/*-linux-gnu/ | grep "libdl" && \
        exit 1; \
    fi && \
    # Copy to all architecture directories
    for ARCH_DIR in aarch64-linux-gnu x86_64-linux-gnu i386-linux-gnu; do \
        mkdir -p "/usr/lib/$ARCH_DIR" && \
        cp -v "$LEPT_SO" "/usr/lib/$ARCH_DIR/libleptonica-1.82.0.so" && \
        cp -v "$TESS_SO" "/usr/lib/$ARCH_DIR/libtesseract50.so" && \
        cp -v "$DL_SO" "/usr/lib/$ARCH_DIR/libdl.so" && \
        chmod +x "/usr/lib/$ARCH_DIR/libleptonica-1.82.0.so" && \
        chmod +x "/usr/lib/$ARCH_DIR/libtesseract50.so" && \
        chmod +x "/usr/lib/$ARCH_DIR/libdl.so" && \
        cd "/usr/lib/$ARCH_DIR" && \
        ln -sf libleptonica-1.82.0.so libleptonica.so && \
        ln -sf libtesseract50.so libtesseract.so; \
    done && \
    # Copy to runtime directories
    for RUNTIME_DIR in arm64 x64 x86; do \
        mkdir -p "/runtimes/linux-$RUNTIME_DIR/native" "/app/$RUNTIME_DIR" && \
        cp -v "$LEPT_SO" "/runtimes/linux-$RUNTIME_DIR/native/libleptonica-1.82.0.so" && \
        cp -v "$TESS_SO" "/runtimes/linux-$RUNTIME_DIR/native/libtesseract50.so" && \
        cp -v "$DL_SO" "/runtimes/linux-$RUNTIME_DIR/native/libdl.so" && \
        cp -v "$LEPT_SO" "/app/$RUNTIME_DIR/libleptonica-1.82.0.so" && \
        cp -v "$TESS_SO" "/app/$RUNTIME_DIR/libtesseract50.so" && \
        cp -v "$DL_SO" "/app/$RUNTIME_DIR/libdl.so" && \
        chmod +x "/runtimes/linux-$RUNTIME_DIR/native/libleptonica-1.82.0.so" && \
        chmod +x "/runtimes/linux-$RUNTIME_DIR/native/libtesseract50.so" && \
        chmod +x "/runtimes/linux-$RUNTIME_DIR/native/libdl.so" && \
        chmod +x "/app/$RUNTIME_DIR/libleptonica-1.82.0.so" && \
        chmod +x "/app/$RUNTIME_DIR/libtesseract50.so" && \
        chmod +x "/app/$RUNTIME_DIR/libdl.so"; \
    done

# Set environment variables for library loading
ENV LD_LIBRARY_PATH=/usr/lib/aarch64-linux-gnu:/usr/lib/x86_64-linux-gnu:/usr/lib/i386-linux-gnu:/app/arm64:/app/x64:/app/x86:/runtimes/linux-arm64/native:/runtimes/linux-x64/native:/runtimes/linux-x86/native
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/5/tessdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Python setup
RUN mv /usr/lib/python3.11/EXTERNALLY-MANAGED /usr/lib/python3.11/EXTERNALLY-MANAGED.old && \
    pip install --upgrade pip && pip install pdfplumber && \
    ln -s $(which python3) /usr/bin/py

ENV PATH="/usr/lib/libreoffice/program:${PATH}"

# Set working directory and copy application files
WORKDIR /app
COPY . /app/

# Final verification
RUN echo "=== Final System State for ${TARGETARCH} ===" && \
    echo "Machine architecture: $(uname -m)" && \
    echo "Library locations:" && \
    ls -la /usr/lib/*/lib*lept* && \
    ls -la /usr/lib/*/lib*tesseract* && \
    ls -la /usr/lib/*/libdl* && \
    echo "Runtime locations:" && \
    ls -la /runtimes/linux-*/native && \
    echo "Tesseract data location:" && \
    ls -la ${TESSDATA_PREFIX}

# Set architecture-specific PROCESSOR_ARCHITECTURE at runtime
RUN echo '#!/bin/sh' > /set-arch.sh && \
    echo 'case $(uname -m) in' >> /set-arch.sh && \
    echo '  aarch64) export PROCESSOR_ARCHITECTURE=ARM64 ;;' >> /set-arch.sh && \
    echo '  x86_64)  export PROCESSOR_ARCHITECTURE=AMD64 ;;' >> /set-arch.sh && \
    echo '  i386|i686) export PROCESSOR_ARCHITECTURE=x86 ;;' >> /set-arch.sh && \
    echo 'esac' >> /set-arch.sh && \
    echo 'exec "$@"' >> /set-arch.sh && \
    chmod +x /set-arch.sh

EXPOSE 8000
ENTRYPOINT ["/set-arch.sh", "dotnet", "DocumentAtom.Server.dll"]