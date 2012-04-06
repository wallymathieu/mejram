require 'json'
require 'rgl/adjacency'

class Relations
    def parse_json_text(text)
        jtables= JSON::parse(text)
        relations = []
        jtables.each do |table|
            columns = table['Columns']
            columns.select do |c|
                c['ColumnName'].end_with?('id')
            end.each do |c|
                val = [c['TableName'].downcase,\
                       c['ColumnName'].gsub('_id','').downcase,\
                       c['ColumnName'].downcase\
                    ]
                relations.push val
            end
        end
        return to_graph(relations)
    end

    def to_graph(relations)
        gr = RGL::DirectedAdjacencyGraph.new
        relations.each do |rel|
            gr.add_edge(rel[0],rel[1])
        end
        return gr
    end
end
