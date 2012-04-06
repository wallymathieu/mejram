import sys
import json

class ProbableForeignKeysAnalysis():
    def __init__(self, table_prefixes=['tbl'], key_names=['id']):
        self.table_prefixes= table_prefixes
        self.key_names=key_names

    def tableNameTrim(self,tablename):
        n = tablename.lower()
        for prefix in self.table_prefixes:
            if n.startswith(prefix):
                i = len(prefix)
                #j = len(n)-i
                return n[i:]
        return n

    def columnExtractTableName(self, columnName):
        n = columnName.lower()
        for key in self.key_names:
            if n.endswith(key):
                return n[0:len(n)-len(key)].rstrip('_')
        return n

    def columnHasKeyName(self, columnName):
        n = columnName.lower()
        return any(filter(lambda key: n.endswith(key),self.key_names))

    def getKeyColumn(self, table):
        tablename = self.tableNameTrim(table['TableName'])
            
        columns = filter(lambda c:
                self.columnHasKeyName(c['ColumnName']),
                table['Columns'])

        withname = filter(lambda c:
                self.columnExtractTableName(c['ColumnName']) == tablename,
                columns)
        for col in withname:
            return col
        raise "No key column found!"
    
    def parse_json_file(self,filename,id):
        try:
            file = open(filename)
            jsondata= file.read()
        finally:
            file.close()
        return self.parse_json_data(jsondata,id)

    def parse_json_data(self,text,id):
        count = 0
        tables = {}
        probable = []
        jtables = json.loads(text)
        for table in jtables:
            tablename = self.tableNameTrim(table['TableName'])
            tables[tablename] = table
            
        for table in jtables:
            tablename = self.tableNameTrim(table['TableName'])
            columns = (filter(lambda c:
                self.columnHasKeyName(c['ColumnName']),
                table['Columns']))
            
            for column in columns:
                columntablename = self.columnExtractTableName(column['ColumnName'])
                if (tablename == columntablename):
                    continue
                
                if (columntablename in tables):
                    foreigntable = tables[columntablename]
                    foreigncolumn = self.getKeyColumn(foreigntable)['ColumnName']
                    probable.append({
                        'from':{
                            'table':table['TableName'],
                            'column':column['ColumnName']},
                        'to':{
                            'table':foreigntable['TableName'],
                            'column':foreigncolumn}
                        })
                    
        return probable

import unittest
class ProbableForeignKeysAnalysisTests(unittest.TestCase):

    def setUp(self):
        self.analysis = ProbableForeignKeysAnalysis()

    def testFormatTableNameWithTblPrefix(self):
        self.assertEqual('store',self.analysis.tableNameTrim('tblstore'))

    def testFormatTableNameWithoutPrefix(self):
        self.assertEqual('store',self.analysis.tableNameTrim('store'))

    def testExtractTableNameFromColumn(self):
        self.assertEqual('store',self.analysis.columnExtractTableName('storeid'))
        self.assertEqual('store',self.analysis.columnExtractTableName('store_id'))
        
    def testStoreTableHasProbableForeignKey(self):
        data = self.analysis.parse_json_file('sakila.Tables.json.txt','id')

        foreignkeysstore = filter(lambda fk:
                    fk['from']['table']=='store',
                    data)
        tables = map(lambda fk:
                    fk['to']['table'],
                    foreignkeysstore)

        self.assertEqual(['address'],list(tables))

if __name__ == '__main__':
    unittest.TextTestRunner().run(unittest.TestLoader().loadTestsFromTestCase(ProbableForeignKeysAnalysisTests))
