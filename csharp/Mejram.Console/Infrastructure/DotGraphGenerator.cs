using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mejram.Models;

namespace Mejram
{
    internal class DotGraphGenerator(
        string @out= "outfile.png",
        string outNeato = "outfile.neato.png",
        string dotfile = "outfile.dot")
    {

        public void GenerateDotFile(ICollection<Table> tables)
        {
            using (var m = File.Open(dotfile, FileMode.Create))
            using (var sout = new StreamWriter(m))
            {
                //size=""90,90""; 
                sout.WriteLine(@"digraph graphname { 
#ratio = square;
node [style=filled];
hexagon [style=bold,style=filled];");
                var conventions= Analysis.TableNameConventions.Default();
                var manyToMany = Analysis.ProbableManyToManyTables(tables, conventions).Where(t=>t.HasMatchingOutgoingForeignKeys());

                foreach (var table in tables)
                {
                    //,style=filled
                    sout.WriteLine("\"{0}\" {1};",
                        table.TableName, table.HasPrimalKey() ? "[shape=hexagon]" : "");
                }
                foreach (var (fromTable, columnName, toTable) in (
                                 from table in tables
                                 from fk in table.ForeignKeys
                                 from fkcol in fk.Columns
                                 select (fkcol.From.TableName, fkcol.From.ColumnName, fkcol.To.TableName)).Distinct())
                {
                    sout.WriteLine("\"{0}\" -> \"{1}\"  [label=\"{2}\"];", fromTable, toTable,
                                   columnName);
                }
                sout.WriteLine("}");
            }
        }

        public void WriteDot(string dotExe)
        {
            var fileName = dotExe;
            var arguments = String.Format("-Tpng {0} -o {1}", dotfile,
                                           @out);
            System.Console.WriteLine("{0} {1}", fileName, arguments);
            var dot = new Process
            {
                StartInfo =
                                  {
                                      FileName = fileName,
                                      Arguments = arguments
                                  }
            };
            dot.Start();
        }

        public void WriteNeato(string neatoExe)
        {
            var arguments = String.Format("-Tpng {0} -o {1}", dotfile,
                                          outNeato);
            var fileName = neatoExe;
            System.Console.WriteLine("{0} {1}", fileName, arguments);
            var dot = new Process
            {
                StartInfo =
                                  {
                                      FileName = fileName,
                                      Arguments = arguments
                                  }
            };
            dot.Start();
        }
    }
}
