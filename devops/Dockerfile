FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime-env
ARG buildconfig
WORKDIR /publish
COPY ./pitaco_publish .
ENTRYPOINT [ "dotnet","Pitaco.Server.dll" ]