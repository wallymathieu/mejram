using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mejram.Model;
using Mejram.NGenerics;

namespace Mejram.Console
{
    public class DotGraphGenerator
    {
        public static void GenerateDotFile(IEnumerable<Table> tables, IEnumerable<ForeignKeyConstraint> foreignKeys)
        {
            using (var m = File.Open(GetOutfileDot(), FileMode.Create))
            using (var sout = new StreamWriter(m))
            {
//size=""90,90""; 
                sout.WriteLine(@"digraph graphname { 
#ratio = square;
node [style=filled];");

                foreach (var table in tables)
                {
                    sout.WriteLine("\"{0}\";", table.TableName); //[shape=box];
                }
                foreach (var key in foreignKeys)
                {
                    var first = key.ConstraintKeys.First();
                    sout.WriteLine("\"{0}\" -> \"{1}\"  [label=\"{2}\"];", first.From.TableName, first.To.TableName,
                                   first.From.ColumnName);
                }
                sout.WriteLine("}");
            }
        }

        public static void WriteDotFile()
        {
            var fileName = ConfigurationManager.AppSettings["dot_exe"];
            var arguments = String.Format("-Tpng {0} -o {1}", GetOutfileDot(),
                                          Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.png"));
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

        public static void WriteNeato()
        {
            var arguments = String.Format("-Tpng {0} -o {1}", GetOutfileDot(),
                                          Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.neato.png"));
            var fileName = ConfigurationManager.AppSettings["neato_exe"];
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

        public static void ComputeGraph(IEnumerable<KeyValuePair<string, Table>> tables1,
                                        IEnumerable<ForeignKeyConstraint> realforeignKeys,
                                        IEnumerable<ForeignKeyConstraint> probableForeignKeys)
        {
            var g = new Graph<string>(false);
            foreach (var tbl in tables1)
            {
                g.AddVertex(tbl.Key.ToLower());
            }

            foreach (var key in realforeignKeys)
            {
                g.AddEdge(g.GetVertex(key.ConstraintKeys.First().From.TableName.ToLower()),
                          g.GetVertex(key.ConstraintKeys.First().To.TableName.ToLower()), 1);
            }
            foreach (var key in probableForeignKeys)
            {
                var first = key.ConstraintKeys.First();
                var vertex = g.GetVertex(first.From.TableName.ToLower());
                var vertex1 = g.GetVertex(first.To.TableName.ToLower());
                if (g.GetEdge(vertex, vertex1) == null)
                    g.AddEdge(vertex, vertex1, 2);
            }
            //v.Compute();
            //var v = GraphAlgorithms.KruskalsAlgorithm(g);
        }

        public static string GetOutfileDot()
        {
            return Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.dot");
        }
    }
}