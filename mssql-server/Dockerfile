FROM mcr.microsoft.com/mssql/server:2019-latest

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=SQLPassword123^&*

# Set up the database
WORKDIR /usr/src/app
COPY setup.sql .
RUN /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -i setup.sql

EXPOSE 1433
