using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Model;
using Mejram.NGenerics;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Data.Common;
using Npgsql;
using Isop;
namespace Mejram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var build = new Build()
                .Recognize(typeof(DotGraphController))
                .Recognize(typeof(GraphController))
                .Recognize(typeof(SerializeController));
            try
            {
                var parsedMethod = build.Parse(args);
                if (parsedMethod.UnRecognizedArguments.Any())//Warning:
                {
                    var unRecognizedArgumentsMessage = string.Format(
                                                           @"Unrecognized arguments: 
{0}
Did you mean any of these arguments?
{1}", String.Join(",", parsedMethod.UnRecognizedArguments.Select(unrec => unrec.Value).ToArray()),
                                                           String.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Name).ToArray()));
                    Console.WriteLine(unRecognizedArgumentsMessage);
                    Environment.Exit(1);
                }
                else
                {
                    parsedMethod.Invoke(Console.Out);
                }
            }
            catch (TypeConversionFailedException ex)
            {

                Console.WriteLine(String.Format("Could not convert argument {0} with value {1} to type {2}",
                        ex.Argument, ex.Value, ex.TargetType));
                if (null != ex.InnerException)
                {
                    Console.WriteLine("Inner exception: ");
                    Console.WriteLine(ex.InnerException.Message);
                }
                Environment.Exit(1);
            }
            catch (MissingArgumentException ex)
            {
                Console.WriteLine(String.Format("Missing argument(s): {0}", String.Join(", ", ex.Arguments.ToArray())));
                if (build.RecognizesHelp)
                    Console.WriteLine(build.Help());
                Environment.Exit(1);
            }
#if DEBUG
            catch (Exception ex1)
            {
                Console.WriteLine(string.Join(Environment.NewLine, new object[]
                        {
                                "The invokation failed with exception:",
                                ex1.Message, ex1.StackTrace
                        }));
                Environment.Exit(1);
            }
#endif

        }
    }
}