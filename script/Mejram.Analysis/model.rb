
class Table
    attr_reader :name, :columns
    def initialize(name, columns)
        @name = name
        @columns = columns
    end
    def to_s
        return "table(#{@name})"
    end
    def ==(other_table)
        if nil == other_table
            return false
        end
        return @name == other_table.name
    end
end

class Column
    attr_reader :name
    def initialize(name)
        @name = name
    end
    def to_s
        return "col(#{@name})"
    end
    def ==(other_column)
        if nil == other_column
            return false
        end
        return @name == other_column.name
    end
end

def parse_json_to_tables(json)
    return json.map do |table|
        Table.new(table['TableName'],\
                  table['Columns'].map do |column|
            Column.new(column['ColumnName']) 
                  end) 
    end
end

