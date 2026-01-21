if [ -z "${IMG_TAG}" ]; then
  IMG_TAG='v1.1.0'
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
#   backups/

# Argument order matters!

docker run \
  -p 8200:8200 \
  -p 8201:8201 \
  -p 8202:8202 \
  -t \
  -i \
  -e "TERM=xterm-256color" \
  -v ./documentatom.json:/app/documentatom.json \
  -v ./logs/:/app/logs/ \
  -v ./backups/:/app/backups/ \
  jchristn77/documentatom-mcp:$IMG_TAG
