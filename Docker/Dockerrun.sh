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

# Argument order matters!

docker run \
  -p 8000:8000 \
  -t \
  -i \
  -e "TERM=xterm-256color" \
  -v ./documentatom.json:/app/documentatom.json \
  jchristn77/documentatom:$IMG_TAG
