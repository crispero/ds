CALL config.cmd

docker run -d --rm -p %BACKENDAPI_PORT%:5000 --name backendapi backendapi:%1
docker run -d --rm -e BACKEND_HOST=%BACKEND_HOST% --link %BACKEND_HOST% -p %MVCMOVIE_PORT%:80 --name mvcmovie mvcmovie:%1