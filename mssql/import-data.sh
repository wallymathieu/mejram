
#wait for the SQL Server to come up
sleep 90s

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Pass@word -d master -i sql-server-sakila-schema.sql

#import the data
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Pass@word -d sakila -i sql-server-sakila-insert-data.sql
