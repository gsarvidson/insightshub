FROM node:22-alpine AS build
WORKDIR /app

COPY package.json yarn.lock .yarnrc.yml ./
COPY .yarn ./.yarn
RUN corepack enable && yarn install --immutable

COPY . .
RUN yarn run build

FROM nginx:alpine
COPY --from=build /app/dist/insights-hub/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
