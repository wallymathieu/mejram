#!/bin/bash
set -e
psql -v ON_ERROR_STOP=1 <<-EOSQL
	CREATE USER sakila;
	CREATE DATABASE sakila;
	GRANT ALL PRIVILEGES ON DATABASE sakila TO sakila;
EOSQL
psql -U postgres -d sakila -f /db-import/postgres-sakila-schema.sql
psql -U postgres -d sakila -f /db-import/postgres-sakila-insert-data.sql
