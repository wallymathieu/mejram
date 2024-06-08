#!/bin/bash

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S mssql -U sa -d master -P ${SA_PASSWORD} -i /db-import/sql-server-sakila-schema.sql

#import the data
/opt/mssql-tools/bin/sqlcmd -S mssql -U sa -d sakila -P ${SA_PASSWORD} -i /db-import/sql-server-sakila-insert-data.sql
