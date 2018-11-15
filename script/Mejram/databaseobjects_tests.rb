$:.unshift File.dirname(__FILE__)

require 'databaseobjects'
require 'minitest/autorun'
require 'dbi'
class PostGresDatabaseObjectsTests # < Minitest::Test
    def setup
        #@conn = PGconn.connect('localhost',5432,'','','sakila','test','test')
        @conn = DBI.connect('DBI:Pg:sakila','test','test')
        @db = DatabaseObjects.new(@conn)
    end
    def teardown
        @conn.disconnect if @conn
    end
    
    def test_can_read_tables
        tables = @db.read_tables
        store = tables.select do |t|
            t['TableName']=='store'
        end.first
        assert_equal({\
            'TableName'=>'store',\
            'Columns'=>[\
{"ColumnName"=>"manager_staff_id", "ColumnType"=>"smallint"},\
{"ColumnName"=>"address_id", "ColumnType"=>"smallint"},\
{"ColumnName"=>"store_id", "ColumnType"=>"integer"},\
{"ColumnName"=>"last_update", "ColumnType"=>"timestamp without time zone"}\
            ]
        },store)
        #    puts tables
    end

    def test_can_read_primary_keys
        pk = @db.read_primary_keys
        #puts pk
    end

    def test_can_read_foreign_keys
        fk = @db.read_foreign_keys
        puts fk
    end
end

