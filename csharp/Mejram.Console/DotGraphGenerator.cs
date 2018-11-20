using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mejram.Models;

namespace Mejram
{
    internal class DotGraphGenerator
    {
        private readonly string _outfilePng;
        private readonly string _outfileNeatoPng;
        private readonly string _outfileDot;

        public DotGraphGenerator()
        {
            _outfilePng = "outfile.png";
            _outfileNeatoPng = "outfile.neato.png";
            _outfileDot = "outfile.dot";
        }

        public void GenerateDotFile(ICollection<Table> tables)
        {
            using (var m = File.Open(_outfileDot, FileMode.Create))
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
                foreach (var key in tables.SelectMany(table=>table.ForeignKeys))
                {
                    var first = key.ForeignKeys.First();
                    sout.WriteLine("\"{0}\" -> \"{1}\"  [label=\"{2}\"];", first.From.TableName, first.To.TableName,
                                   first.From.ColumnName);
                }
                sout.WriteLine("}");
            }
        }

        public void WriteDot(string dotExe)
        {
            var fileName = dotExe;
            var arguments = String.Format("-Tpng {0} -o {1}", _outfileDot,
                                           _outfilePng);
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
            var arguments = String.Format("-Tpng {0} -o {1}", _outfileDot,
                                          _outfileNeatoPng);
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
