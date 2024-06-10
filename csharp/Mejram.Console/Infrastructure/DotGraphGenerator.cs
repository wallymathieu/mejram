using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mejram.Models;

namespace Mejram
{
    internal class DotGraphGenerator(
        string @out= "outfile.png",
        string outNeato = "outfile.neato.png",
        string dotfile = "outfile.dot")
    {

        public async Task GenerateDotFile(ICollection<Table> tables)
        {
            using var m = File.Open(dotfile, FileMode.Create);
            using var sout = new StreamWriter(m);
            //size=""90,90""; 
            await sout.WriteLineAsync(@"digraph graphname { 
#ratio = square;
node [style=filled];
hexagon [style=bold,style=filled];");


            foreach (var table in tables)
            {
                //,style=filled
                await sout.WriteLineAsync(
                    $"\"{table.TableName}\" {(table.HasPrimalKey() ? "[shape=hexagon]" : "")};");
            }
            foreach (var (fromTable, columnName, toTable) in (
                             from table in tables
                             from fk in table.ForeignKeys
                             from fkcol in fk.Columns
                             select (fkcol.From.TableName, fkcol.From.ColumnName, fkcol.To.TableName)).Distinct())
            {
                await sout.WriteLineAsync($"\"{fromTable}\" -> \"{toTable}\"  [label=\"{columnName}\"];");
            }
            await sout.WriteLineAsync("}");
        }

        public async Task WriteDot(string dotExe)
        {
            var fileName = dotExe;
            var arguments = $"-Tpng {dotfile} -o {@out}";
            await Console.Out.WriteLineAsync(string.Format("{0} {1}", fileName, arguments));
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

        public async Task WriteNeato(string neatoExe)
        {
            var arguments = $"-Tpng {dotfile} -o {outNeato}";
            var fileName = neatoExe;
            await Console.Out.WriteLineAsync(string.Join($"{fileName} {arguments}"));
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
