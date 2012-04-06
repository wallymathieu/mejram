require 'json'

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
        tablename = tableNameTrim(table['TableName'])

        columns = table['Columns'].select do |c| 
            columnHasKeyName(c['ColumnName'])
        end
        withname = columns.select do |c| 
            columnExtractTableName(c['ColumnName']) == tablename
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
        jtables = JSON::parse(text)

        jtables.each do |table|
            tablename = tableNameTrim(table['TableName'])
            tables[tablename] = table
        end 

        jtables.each do |table|
            tablename = tableNameTrim(table['TableName'])
            columns = table['Columns'].select do |c|
                columnHasKeyName(c['ColumnName'])\
                    && columnExtractTableName(c['ColumnName'])!=tablename
            end
            columns.each do |column| 
                columntablename = columnExtractTableName(column['ColumnName'])
                if tables.has_key?(columntablename) 
                    foreigntable = tables[columntablename]
                    foreigncolumn = getKeyColumn(foreigntable)['ColumnName']
                    probable.push({
                        'from'=>{
                        'table'=>table['TableName'],
                        'column'=>column['ColumnName']},
                        'to'=>{
                        'table'=>foreigntable['TableName'],
                        'column'=>foreigncolumn}
                    })
                end
            end
        end            
        return probable
    end
end

