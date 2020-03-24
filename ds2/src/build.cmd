docker build -t backendapi:%1 -f Dockerfile.backendapi .
docker build -t mvcmovie:%1 -f Dockerfile.mvcmovie .

MKDIR product%1

COPY start.cmd product%1
COPY stop.cmd product%1
COPY config.cmd product%1