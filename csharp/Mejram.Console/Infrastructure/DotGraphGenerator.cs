using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mejram.Models;
using static Mejram.Infrastructure.Tables;
namespace Mejram.Infrastructure
{
    ///<summary> Writes graph files. See <a href="https://graphviz.org/documentation/">Graphviz documentation</a>.</summary>
    internal class DotGraphGenerator(
        string dotfile = "outfile.dot")
    {
 
        // References:
        // https://stackoverflow.com/questions/13417411/laying-out-a-large-graph-with-graphviz
        // https://stackoverflow.com/questions/16173764/what-is-the-best-way-to-draw-large-graph-using-graphvis

        public void GenerateDotFile(ICollection<Table> tables)
        {
            using var m = File.Open(dotfile, FileMode.Create);
            using var sout = new StreamWriter(m);
            //size=""90,90""; 
            sout.WriteLine(@"digraph graphname { 
splines=true ;overlap=false;
node [style=filled];
");
            var conventions = Analysis.TableNameConventions.Default();
            var relatedTable = RelatedTables(tables);
          
            foreach (var table in tables.Where(t => relatedTable.Contains(t.TableName)))
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

        public void GenerateDotFileWithLessManyToMany(ICollection<Table> tables)
        {
            using var m = File.Open(dotfile, FileMode.Create);
            using var sout = new StreamWriter(m);
            //size=""90,90""; 
            sout.WriteLine(@"strict graph graphname { 
splines=true ;overlap=false;
node [style=filled];
");
            var relatedTable = RelatedTables(tables);
            var conventions = Analysis.TableNameConventions.Default();
            var manyToMany = Analysis.ProbableManyToManyTables(tables, conventions).Where(t => t.HasMatchingOutgoingForeignKeys());
            var manyToManyNames = manyToMany.Select(t => t.Table.TableName).ToHashSet();
            bool IsAManyToMany(Table t) => manyToManyNames.Contains(t.TableName);
            foreach (var table in tables.Where(t => !IsAManyToMany(t) && relatedTable.Contains(t.TableName)))
            {
                //,style=filled
                sout.WriteLine("\"{0}\" {1};",
                    table.TableName, table.HasPrimalKey() ? "[shape=hexagon]" : "");
            }
            foreach (var (fromTable, toTable, connectingTable) in (
                             from mm in manyToMany
                             select (mm.FirstOtherTable, mm.SecondOtherTable, mm.Table.TableName)).Distinct())
            {
                sout.WriteLine("\"{0}\" -- \"{1}\" [label=\"{2}\"] ; ", fromTable, toTable, "*");
            }
            foreach (var (fromTable, columnName, toTable) in (
                             from table in tables
                             where !IsAManyToMany(table)
                             from fk in table.ForeignKeys
                             from fkcol in fk.Columns
                             select (fkcol.From.TableName, fkcol.From.ColumnName, fkcol.To.TableName)).Distinct())
            {
                sout.WriteLine("\"{0}\" -- \"{1}\"  [label=\"{2}\"];", fromTable, toTable,
                               columnName);
            }
            sout.WriteLine("}");
        }

       

        public void WriteDot(string dotExe, string @out= "outfile.png")
        {
            var fileName = dotExe;
            var arguments = String.Format(" -x -Goverlap=scale -Tpng {0} -o {1}", dotfile,
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

        public void WriteNeato(string neatoExe, string outNeato = "outfile.neato.png")
        {
            var arguments = String.Format(" -x -Goverlap=scale -Tpng {0} -o {1}", dotfile,
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
