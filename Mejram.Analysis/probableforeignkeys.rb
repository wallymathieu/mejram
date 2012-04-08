$:.unshift File.dirname(__FILE__)
require 'json'
require 'model'
class ProbableForeignKeysAnalysis
    def initialize(table_prefixes=['tbl'], key_names=['id'])
        @table_prefixes= table_prefixes
        @key_names=key_names
    end

    def tableNameTrim(tablename)
        n = tablename.downcase()
        @table_prefixes.each do |prefix|
            if n.start_with?(prefix)
                i = prefix.length
                return n[i .. n.length]
            end
        end
        return n
    end

    def columnExtractTableName(columnName)
        n = columnName.downcase()
        @key_names.each do |key|
            if n.end_with?(key)
                return n[0..(n.length)-key.length-1].gsub(/_$/,'')
            end
        end
        return n
    end

    def columnHasKeyName(columnName)
        n = columnName.downcase()
        return @key_names.select do |key| n.end_with?(key) end
    end

    def getKeyColumn(table)
        tablename = tableNameTrim(table.name)

        columns = table.columns.select do |c| 
            columnHasKeyName(c.name)
        end
        withname = columns.select do |c| 
            columnExtractTableName(c.name) == tablename
        end
        return withname.first
    end
    def parseJsonFile(filename,id)
        return parseJsonData(File.open(filename).read,id)
    end
    def parseJsonData(text,id)
        count = 0
        tables = {}
        probable = []
        jtables = parse_json_to_tables(JSON::parse(text))

        jtables.each do |table|
            tablename = tableNameTrim(table.name)
            tables[tablename] = table
        end 

        jtables.each do |table|
            tablename = tableNameTrim(table.name)
            columns = table.columns.select do |c|
                columnHasKeyName(c.name)\
                    && columnExtractTableName(c.name)!=tablename
            end
            columns.each do |column| 
                columntablename = columnExtractTableName(column.name)
                if tables.has_key?(columntablename) 
                    foreigntable = tables[columntablename]
                    foreigncolumn = getKeyColumn(foreigntable).name
                    probable.push({
                        'from'=>{
                        'table'=>table.name,
                        'column'=>column.name},
                        'to'=>{
                        'table'=>foreigntable.name,
                        'column'=>foreigncolumn}
                    })
                end
            end
        end            
        return probable
    end
end

