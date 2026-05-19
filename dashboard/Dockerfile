FROM node:22-alpine AS build
WORKDIR /repo
RUN apk add --no-cache curl
COPY sdk/typescript/ ./sdk/typescript/
COPY dashboard/ ./dashboard/
RUN printf '#!/bin/sh\nexit 0' > /usr/local/bin/husky && chmod +x /usr/local/bin/husky
WORKDIR /repo/sdk/typescript
RUN npm ci --omit=dev
WORKDIR /repo/dashboard
RUN npm ci
RUN npm run build
RUN npm prune --omit=dev

FROM node:22-alpine AS runtime
WORKDIR /app
RUN apk add --no-cache curl
COPY --from=build /repo/dashboard/.next ./.next
COPY --from=build /repo/dashboard/public ./public
COPY --from=build /repo/dashboard/package.json ./package.json
COPY --from=build /repo/dashboard/package-lock.json ./package-lock.json
COPY --from=build /repo/dashboard/next.config.mjs ./next.config.mjs
COPY --from=build /repo/dashboard/node_modules ./node_modules
EXPOSE 3000
CMD [ "npm", "run", "start" ]
