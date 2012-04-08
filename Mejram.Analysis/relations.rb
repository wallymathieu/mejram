$:.unshift File.dirname(__FILE__)
require 'model'
require 'json'
require 'rgl/adjacency'

class Relations
    def parse_json_text(text)
        jtables= parse_json_to_tables(JSON::parse(text))
        relations = []
        jtables.each do |table|
            columns = table.columns
            columns.select do |c|
                c.name.end_with?('id')
            end.each do |c|
                val = [table.name.downcase,\
                       c.name.gsub(/_?id/,'').downcase,\
                       c.name.downcase\
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
