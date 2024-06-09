#!/usr/bin/env bash
# make sure to install sqlcmd:
# https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility?view=sql-server-ver16&tabs=go%2Cwindows&pivots=cs1-bash#download-and-install-sqlcmd
pushd $(dirname "${0}") > /dev/null
cd ../
MSSQL_HOST=localhost
MSSQL_PORT=5433
source .env

function mssql_select_1 {
    sqlcmd -S "${MSSQL_HOST},${MSSQL_PORT}" -U sa -P "$MSSQL_SA_PASSWORD" -Q 'SELECT 1'
    return $?
}

function mssql_wait {
  echo -n "Waiting for mssql to come up"
  until ( mssql_select_1 ) &>/dev/null; do
    echo -n '.' && sleep 0.2
  done
  echo " Ok"
}

function mssql_import_schema {
    #run the setup script to create the DB and the schema in the DB
    for VARIABLE in 1 2 
    do
        sqlcmd -S "${MSSQL_HOST},${MSSQL_PORT}" -d master -U sa -P "$MSSQL_SA_PASSWORD" -i ./sakila/sql-server-sakila-db/sql-server-sakila-schema.sql
    done
}


function mssql_import_data {
    if sqlcmd -S "${MSSQL_HOST},${MSSQL_PORT}" -d sakila -U sa -P "$MSSQL_SA_PASSWORD" -i ./sakila/sql-server-sakila-db/sql-server-sakila-insert-data.sql ; then
        echo "Failed to import sakila data"
        exit 1
    fi
}

#mssql_select_1
mssql_wait
mssql_import_schema
mssql_import_data
