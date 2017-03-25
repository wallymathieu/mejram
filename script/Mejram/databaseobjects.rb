
class DatabaseObjects
    def initialize(conn)
        @conn = conn
        @valids = ["yes", "true", "1"]
    end

    def read_primary_keys
        pk = {}
        sql = "
SELECT 
pkcon.constraint_name,
pkcon.table_name, 
pkcon_col.column_name
FROM 
information_schema.table_constraints pkcon 
JOIN information_schema.constraint_column_usage pkcon_col ON pkcon_col.constraint_name = pkcon.constraint_name
JOIN information_schema.tables t
    ON pkcon.table_name = t.table_name  
WHERE 
pkcon.constraint_type = 'PRIMARY KEY'
AND
#{with_tables('t')}
"       
        res = @conn.execute(sql)
        while row = res.fetch do
            pkname = row[0]
            tablename = row[1]
            columnname = row[2]
            if pk.has_key?(pkname)
                pk[pkname] = {\
                    'ConstraintName'=>pkname,\
                    'TableName'=>tablename,\
                    'ConstraintKeys'=>[]\
                }
            end
            pk[pkname]['ConstraintKeys'].push(columnname)
        end
        res.finish
        return pk.values
    end
    def with_tables(alias_)
        return "
        (#{alias_}.TABLE_SCHEMA <> 'pg_catalog'
        AND #{alias_}.TABLE_SCHEMA <> 'information_schema')
        "
    end
        
    def read_tables
         sql = "
SELECT c.table_name, 
        c.column_name, 
        c.data_type,  
        c.ordinal_position,
        c.is_nullable
FROM INFORMATION_SCHEMA.COLUMNS c  
INNER JOIN information_schema.tables t
    ON c.table_name = t.table_name  
WHERE 
#{with_tables('t')}
"
        tables = {}
        res = @conn.execute(sql)
        while row = res.fetch do
            table_name = row[0]
            if ! tables.has_key?(table_name)
                tables[table_name] = {\
                    'TableName'=>table_name,\
                    'Columns'=>[]\
                }
            end
            col = {\
                'ColumnName'=>row[1],\
                'ColumnType'=>row[2]\
            }

            tables[table_name]['Columns'].push(col)
        end
        res.finish
        return tables.values
    end

    def read_foreign_keys
        cons = {}
sql = "
SELECT
    con.constraint_name,
    con.unique_constraint_name, 
    tcon.table_name tcon_table_name,
    pkcon.table_name pkcon_table_name,
    con_col.column_name con_col_column_name,
    pkcon_col.column_name pkcon_col_column_name
FROM 
    information_schema.referential_constraints con
    JOIN information_schema.table_constraints tcon
        ON con.constraint_name = tcon.constraint_name
    JOIN information_schema.table_constraints pkcon 
        ON con.unique_constraint_name = pkcon.constraint_name
	    AND pkcon.constraint_type = 'PRIMARY KEY'
    JOIN information_schema.constraint_column_usage con_col
        ON con_col.constraint_name = con.constraint_name 
    JOIN information_schema.constraint_column_usage pkcon_col 
        ON pkcon_col.constraint_name = pkcon.constraint_name
    JOIN information_schema.tables t 
        ON t.table_name = tcon.table_name
WHERE 
#{with_tables('t')}
"
        res = @conn.execute(sql)
        while row = res.fetch do
            ucname = row[0]
            cotablename = row[1]
            pktablename = row[2]
            cocolumnname = row[3]
            pkcolumnname = row[4]
            if cons.has_key?(ucname)
                cons[ucname] = {'From'=>{\
                    'TableName'=>cotablename,\
                    'ColumnName'=>cocolumnname\
                },\
                'To'=>{\
                    'TableName'=>pktablename,\
                    'ColumnName'=>pkcolumnname\
                }\
                }
            else
                raise "! not implemented"
            end
        end 
        res.finish
        return cons
    end
end


