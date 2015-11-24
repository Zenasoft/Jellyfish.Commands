FROM microsoft/aspnet:1.0.0-rc1-final

COPY . /app
WORKDIR app

EXPOSE 5000

RUN ["dnu", "restore", "-s", "https://nuget.org/api/v2/", "-s", "https://www.myget.org/F/jellyfish/api/v2", "-s", "https://www.myget.org/F/aspnetvnext/api/v2"]
RUN ["dnu", "build"]

CMD ["dnx", "kestrel"]