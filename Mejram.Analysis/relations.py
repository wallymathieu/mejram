import sys
# http://code.google.com/p/python-graph/ 
from pygraph.classes.graph import graph
from pygraph.algorithms.searching import breadth_first_search
import json

class Relations():
    def parse_json_file(self,fileName):
        try:
            file = open(fileName)
            jsondata= file.read()
        finally:
            file.close()
        return self.parse_json_data(jsondata)
    
    def parse_json_data(self,text):
        relations = []
        tables = json.loads(text)
        for table in tables:
            for column in (filter(lambda c: c['ColumnName'].endswith('id'),table['Columns'])):
                val = [ column['TableName'].lower(),
                        column['ColumnName'].replace('_id','').lower(),
                        column['ColumnName'].lower()
                    ]
                relations.append(val)
        
        return self.get_graph(relations)

    def get_graph(self,relations):
        gr=graph()

        for row in relations:
                if (not gr.has_node(row[0])):
                        gr.add_node(row[0])
                if (not gr.has_node(row[1])):
                        gr.add_node(row[1])
                gr.add_edge((row[0],row[1]))
                gr.set_edge_label((row[0],row[1]),row[2])
        return gr

import unittest
class RelationsTests(unittest.TestCase):

    def setUp(self):
        self.rel = Relations()

    def test_breadth_first_search(self):
        gr = self.rel.parse_json_file('sakila.Tables.json.txt')
        st, order = breadth_first_search(gr,root='store')
        print (st)
        print (order)

if __name__ == '__main__':
    unittest.TextTestRunner().run(unittest.TestLoader().loadTestsFromTestCase(RelationsTests))
