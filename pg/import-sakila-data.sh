#!/usr/bin/env bash
# make sure to install psql (PostgreSQL client)
pushd $(dirname "${0}") > /dev/null
cd ../
PG_HOST=localhost
PG_PORT=5434
source .env

set -e
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -v ON_ERROR_STOP=1 <<-EOSQL
	CREATE USER sakila;
	CREATE DATABASE sakila;
	GRANT ALL PRIVILEGES ON DATABASE sakila TO sakila;
EOSQL
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -d sakila -f ./sakila/postgres-sakila-db/postgres-sakila-schema.sql
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -d sakila -f ./sakila/postgres-sakila-db/postgres-sakila-insert-data.sql
