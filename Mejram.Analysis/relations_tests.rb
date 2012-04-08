$:.unshift File.dirname(__FILE__)
require 'relations'
require 'test/unit'

require 'rgl/traversal'
require 'rgl/dot'
class RelationsTest < Test::Unit::TestCase
    def setup
        text = File.open('sakila.Tables.json.txt').read 
        @tables= parse_json_to_tables(JSON::parse(text))
        @rel = Relations.new 
    end

    def test_graph_traversal
        p=@rel.analyse(@tables)
        #p.write_to_graphic_file
        t = p.bfs_search_tree_from(p.detect{ |x| 
            x == 'store' 
        })
        #t.dotty
        #t.write_to_graphic_file
    end
end
