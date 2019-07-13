FROM microsoft/mssql-server-linux:latest

# Create app directory
RUN mkdir -p /db-import

# Copy setup scripts
COPY ./jOOQ/jOOQ-examples/Sakila/sql-server-sakila-db /db-import
ENV SA_PASSWORD=Pass@word
RUN /opt/mssql/bin/sqlservr --accept-eula & sleep 20 \
    && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d master -i /db-import/sql-server-sakila-schema.sql \
    && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d sakila -i /db-import/sql-server-sakila-insert-data.sql \
    && pkill sqlservr
CMD /opt/mssql/bin/sqlservr --accept-eula
