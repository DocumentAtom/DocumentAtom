#!/bin/bash

if [ -z "${IMG_TAG}" ]; then
  IMG_TAG='v1.0.0'
fi

echo Using image tag $IMG_TAG

if [ ! -f "documentatom.json" ]
then
  echo Configuration file documentatom.json not found.
  exit
fi

# Items that require persistence
#   documentatom.json
#   logs/
#   temp/
#   backups/

# Argument order matters!

docker run \
  -p 8000:8000 \
  -p 8001:8001 \
  -p 8002:8002 \
  -t \
  -i \
  -e "TERM=xterm-256color" \
  -v ./documentatom.json:/app/documentatom.json \
  -v ./logs/:/app/logs/ \
  -v ./temp/:/app/temp/ \
  -v ./backups/:/app/backups/ \
  jchristn77/documentatom-mcp:$IMG_TAG
