using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mejram.Models;

namespace Mejram
{
    internal class MermaidGraphGenerator(
        string mermaidFile = "outfile.mmd")
    {
        public void GenerateFile(ICollection<Table> tables)
        {
            using var m = File.Open(mermaidFile, FileMode.Create);
            using var sout = new StreamWriter(m);
            //size=""90,90""; 
            sout.WriteLine(@"graph TD");
            var conventions = Analysis.TableNameConventions.Default();
            
            foreach (var table in tables)
            {
                //,style=filled
                sout.WriteLine("{0}[{1}{0}];",
                    table.TableName, table.HasPrimalKey() ? "fa:fa-bullseye " : "");
            }
            foreach (var (fromTable, columnName, toTable) in (
                             from table in tables
                             from fk in table.ForeignKeys
                             from fkcol in fk.Columns
                             select (fkcol.From.TableName, fkcol.From.ColumnName, fkcol.To.TableName)).Distinct())
            {
                sout.WriteLine("{0} --> |{2}| {1}", fromTable, toTable,
                               columnName);
            }
            sout.WriteLine("");
        }

        public void Write(string mermaidExecutable, string @out= "outfile.mmd.svg")
        {
            var fileName = mermaidExecutable;
            var arguments = String.Format(" -w 15000 -i {0} -o {1}", mermaidFile,
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

    }
}
