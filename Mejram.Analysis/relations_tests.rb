$:.unshift File.dirname(__FILE__)
require 'relations'
require 'test/unit'

class RelationsTest < Test::Unit::TestCase
    def setup
        @content = File.open('sakila.Tables.json.txt').read 
        @rel = Relations.new 
    end

    def test_f
        p=@rel.parse_json_text(@content)
        puts p
    end
end
