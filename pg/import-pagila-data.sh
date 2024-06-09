#!/usr/bin/env bash
# make sure to install psql (PostgreSQL client)
pushd $(dirname "${0}") > /dev/null
cd ../
PG_HOST=localhost
PG_PORT=5434
source .env

set -e
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -c "CREATE DATABASE sakila;"
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -d sakila -f ./pagila/pagila-schema.sql
PGPASSWORD=$POSTGRES_PASSWORD psql -h $PG_HOST -p $PG_PORT -U postgres -d sakila -f ./pagila/pagila-insert-data.sql
