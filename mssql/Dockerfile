FROM microsoft/mssql-server-linux:latest

# Create app directory
RUN mkdir -p /db-import
WORKDIR /db-import

# Copy setup scripts
COPY ./jOOQ/jOOQ-examples/Sakila/sql-server-sakila-db .
COPY ./mssql/import-data.sh .
# Grant permissions for the import-data script to be executable
RUN chmod +x ./import-data.sh
RUN /opt/mssql/bin/sqlservr --accept-eula & sleep 20 \
    && /db-import/import-data.sh \
    && pkill sqlservr 
CMD /opt/mssql/bin/sqlservr --accept-eula
