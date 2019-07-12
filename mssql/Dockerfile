FROM microsoft/mssql-server-linux:latest

# Create app directory
RUN mkdir -p /db-import
WORKDIR /db-import

# Copy setup scripts
COPY ./jOOQ/jOOQ-examples/Sakila/sql-server-sakila-db .
COPY ./mssql/import-data.sh .
COPY ./mssql/entrypoint.sh .
# Grant permissions for the import-data script to be executable
RUN chmod +x ./import-data.sh

CMD /bin/bash ./entrypoint.sh
