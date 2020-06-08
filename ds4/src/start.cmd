CALL config.cmd
docker run -d --rm -p %NATS_PORT%:4222 --name nats nats
docker run -d --rm -p %REDIS_PORT%:6379 --name redis redis
docker run -d --rm -e NATS_HOST=%NATS_HOST% -e NATS_PORT=%NATS_PORT% -e REDIS_HOST=%REDIS_HOST% -e REDIS_PORT=%REDIS_PORT% -e NATS_EVENT_BUS=%NATS_EVENT_BUS% --link %NATS_HOST% --link %REDIS_HOST% -p %BACKEND_API_PORT%:5000 --name backendapi backendapi:%1
docker run -d --rm -e BACKEND_HOST=%BACKEND_HOST% --link %BACKEND_HOST% -p %MVCMOVIE_PORT%:80 --name mvcmovie mvcmovie:%1
docker run -d --rm -e NATS_HOST=%NATS_HOST% -e NATS_PORT=%NATS_PORT% -e REDIS_HOST=%REDIS_HOST% -e REDIS_PORT=%REDIS_PORT% -e NATS_EVENT_BUS=%NATS_EVENT_BUS% --link %NATS_HOST% --link %REDIS_HOST% --name joblogger joblogger:%1
docker run -d --rm -e NATS_HOST=%NATS_HOST% -e NATS_PORT=%NATS_PORT% -e REDIS_HOST=%REDIS_HOST% -e REDIS_PORT=%REDIS_PORT% -e NATS_EVENT_BUS=%NATS_EVENT_BUS% --link %NATS_HOST% --link %REDIS_HOST% --name textrankcalc textrankcalc:%1