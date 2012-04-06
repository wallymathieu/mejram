
$:.unshift File.dirname(__FILE__)

require 'probableforeignkeys'
require 'test/unit'
#import unittest
class ProbableForeignKeysAnalysisTests < Test::Unit::TestCase

    def setup()
        @analysis = ProbableForeignKeysAnalysis.new()
    end

    def testFormatTableNameWithTblPrefix
        assert_equal('store',@analysis.tableNameTrim('tblstore'))
    end

    def testFormatTableNameWithoutPrefix()
        assert_equal('store',@analysis.tableNameTrim('store'))
    end

    def testExtractTableNameFromColumn()
        assert_equal('store',@analysis.columnExtractTableName('storeid'))
        assert_equal('store',@analysis.columnExtractTableName('store_id'))
    end 

    def testStoreTableHasProbableForeignKey()
        data = @analysis.parseJsonFile('sakila.Tables.json.txt','id')

        foreignkeysstore = data.select do |fk|
            fk['from']['table']=='store'
        end
        tables =  foreignkeysstore.map do |fk|
            fk['to']['table']
        end

        assert_equal(['address'],tables)
    end
end
