FROM microsoft/aspnet:1.0.0-beta7

COPY . /app
WORKDIR app

RUN ["dnu", "restore"]
RUN ["dnu", "build"]

CMD ["dnx", "kestrel"]