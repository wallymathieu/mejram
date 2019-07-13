#!/bin/bash
#!/bin/bash
#sleep 10s

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -d master -i /db-import/sql-server-sakila-schema.sql

#import the data
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -d sakila -i /db-import/sql-server-sakila-insert-data.sql
