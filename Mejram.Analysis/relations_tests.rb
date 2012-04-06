$:.unshift File.dirname(__FILE__)
require 'relations'
require 'test/unit'

require 'rgl/traversal'

class RelationsTest < Test::Unit::TestCase
    def setup
        @content = File.open('sakila.Tables.json.txt').read 
        @rel = Relations.new 
    end

    def test_graph_traversal
        p=@rel.parse_json_text(@content)
        puts p.bfs_search_tree_from(p.detect{ |x| 
            x.methods; x == 'store' 
        })
    end
end
