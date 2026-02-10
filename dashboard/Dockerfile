FROM node:22-slim
WORKDIR /repo
RUN apt-get update && apt-get install -y iputils-ping traceroute net-tools curl wget dnsutils iproute2 file vim procps xsel && rm -rf /var/lib/apt/lists/*
COPY sdk/typescript/ ./sdk/typescript/
COPY dashboard/ ./dashboard/
RUN printf '#!/bin/sh\nexit 0' > /usr/local/bin/husky && chmod +x /usr/local/bin/husky
WORKDIR /repo/sdk/typescript
RUN npm install --omit=dev
WORKDIR /repo/dashboard
RUN npm install
RUN npm run build
EXPOSE 3000
CMD [ "npm", "run", "start" ]
