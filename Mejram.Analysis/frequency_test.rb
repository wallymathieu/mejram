$:.unshift File.dirname(__FILE__)
require 'frequency'
require 'test/unit'

class FrequencyTest < Test::Unit::TestCase
    def setup
        text = File.open('sakila.Tables.json.txt').read
        tables= parse_json_to_tables(JSON::parse(text))
        @freq = Frequency.new(tables)
    end

    def test_can_get_key_frequency
        keys = @freq.key_frequency
         expected = {:single_keys=>{},\
:compound_keys=>{"id"=>13, "date"=>2, "2"=>1},\
:underscore=>15,\
:non_underscore=>1}

        assert_equal(expected,keys)
    end

    #def test_can_give_format
    #    format = @freq.key_format
    #    assert_equal('{tablename}_id',format)
    #end
end
