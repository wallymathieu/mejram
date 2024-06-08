FROM mcr.microsoft.com/mssql-tools

# Create app directory
RUN mkdir -p /db-import
ENV SA_PASSWORD "Pass@word"
# Copy setup scripts
COPY ./jOOQ/jOOQ-examples/Sakila/sql-server-sakila-db /db-import
COPY ./mssql/import-data.sh /db-import
RUN ./db-import/import-data.sh
