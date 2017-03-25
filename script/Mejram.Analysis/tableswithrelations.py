import sys
import json

class TablesWithRelations():
    def parseJsonFile(self,fileName,id):
        try:
            file = open(fileName)
            jsondata= file.read()
        finally:
            file.close()
        return self.parseJsonData(jsondata,id)

    def parseJsonData(self,text,id):
        count = 0
        tables = {}
        jtables = json.loads(text)
        for table in jtables:
            for column in (filter(lambda c:
                c['ColumnName'].endswith(id)
                and c['TableName'].lower()!= c['ColumnName'].replace(id,'').lower().strip('_'),
                table['Columns'])):

                key = column['TableName'].lower()
                val = column['ColumnName'].lower()
                if (key not in tables): tables[key] = []
                    
                tables[key].append(val)
        return tables

import unittest
class TablesWithRelationsTests(unittest.TestCase):

    def setUp(self):
        self.conn = TablesWithRelations()

    def testSomeTable(self):
        data = self.conn.parseJsonFile('sakila.Tables.json.txt','id')
        #print(data)
        c =data['customer']
        #print(dir(c))
        self.assertEqual(['store_id','address_id'],c)

if __name__ == '__main__':
    unittest.TextTestRunner().run(unittest.TestLoader().loadTestsFromTestCase(TablesWithRelationsTests))
