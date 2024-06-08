# https://stackoverflow.com/questions/62667711/how-to-run-docker-based-mssql-tools-using-a-regular-way
FROM mcr.microsoft.com/mssql-tools
# todo see if there are better ways ... https://github.com/search?q=mcr.microsoft.com%2Fmssql-tools+&type=code
# Create app directory
RUN mkdir -p /db-import
ENV SA_PASSWORD "Pass@word"
# Copy setup scripts
COPY ./jOOQ/jOOQ-examples/Sakila/sql-server-sakila-db /db-import
COPY ./mssql/import-data.sh /db-import
RUN ./db-import/import-data.sh
