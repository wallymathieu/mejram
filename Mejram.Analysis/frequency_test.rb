$:.unshift File.dirname(__FILE__)
require 'frequency'
require 'test/unit'

class FrequencyTest < Test::Unit::TestCase
    def setup
        @content = File.open('sakila.Tables.json.txt').read
        @freq = Frequency.new(@content)
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
