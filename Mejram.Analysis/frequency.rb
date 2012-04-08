$:.unshift File.dirname(__FILE__)
require 'model'
require 'json'

class Counter
    def initialize()
        @count = {}
    end

    def acc(key)
        if ! @count.has_key?(key)
            @count[key] = 0
        end
        @count[key] += 1
    end
    def to_hash
        return @count.clone
    end
end

class Frequency
    #attr
    def initialize(text)
        tables= parse_json_to_tables(JSON::parse(text))
        @tables = tables
    end

    def key_frequency
        keys = ['id']
        compound_keys = Counter.new
        count = Counter.new
        underscore = 0
        non_underscore = 0
        tablenames = @tables.map do |t|
            t.name
        end
        @tables.each do |t|
            column_names = t.columns.map do |c|
                c.name.downcase()
            end
            matching_key =column_names.select do |n|
                keys.index(n)
            end

            if matching_key.length==1
                count.acc(mathing_key[0])
            elsif matching_key.length > 1
                raise "! ambigous key"
            elsif matching_key.length == 0
                r = Regexp.new("#{t.name}(_{,1})(\\w+)",Regexp::IGNORECASE)
                matches = column_names.select do |c|
                    r.match(c)
                end

                matches.each do |m|
                    match = r.match(m)
                    if match[1].length>0
                        underscore +=1
                    else
                        non_underscore +=1
                    end
                    compound_keys.acc(match[2])
                end
            end
        end
        return {:single_keys=>count.to_hash,\
            :compound_keys=>compound_keys.to_hash,\
            :underscore=>underscore,\
            :non_underscore=> non_underscore }
    end
end
