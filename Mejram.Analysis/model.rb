
class Table
    attr_reader :name, :columns
    def initialize(name, columns)
        @name = name
        @columns = columns
    end
end

class Column
    attr_reader :name
    def initialize(name)
        @name = name
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

