#!/usr/bin/env bash
pushd $(dirname "${0}") > /dev/null
cd ../
MSSQL_HOST=mssql
MSSQL_PORT=1433
source .env
MSSQL_EXEC=" --net=mejram_default mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd "
SAKILA_VOLUME="$(pwd)/sakila/sql-server-sakila-db:/db-import"

function mssql_select_1 {
    # docker run --rm -it --net=mejram_default mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S 'mssql,1433' -U sa -P 'SET_A_PASSWORD_HERE_123' -Q 'SELECT 1'
    # docker run --rm -it --net=mejram_default mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S 'mssql,1433' -U sa -P 'SET_A_PASSWORD_HERE_123' -Q SELECT 1
    docker run --rm -it ${MSSQL_EXEC} -S "${MSSQL_HOST},${MSSQL_PORT}" -U sa -P "$MSSQL_PASSWORD" -Q 'SELECT 1'
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
        docker run --rm -it -v ${SAKILA_VOLUME} ${MSSQL_EXEC} -S "${MSSQL_HOST},${MSSQL_PORT}" -d master -U sa -P "$MSSQL_PASSWORD" -i /db-import/sql-server-sakila-schema.sql
    done
}


function mssql_import_data {
    #import the data
    if echo docker run --rm -it -v ${SAKILA_VOLUME} ${MSSQL_EXEC} -S "${MSSQL_HOST},${MSSQL_PORT}" -d sakila -U sa -P "$MSSQL_PASSWORD" -i /db-import/sql-server-sakila-insert-data.sql ; then
        echo "Failed to import sakila data"
        exit 1
    fi
}

#mssql_select_1
mssql_wait
mssql_import_schema
mssql_import_data