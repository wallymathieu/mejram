using System;
using System.Linq;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;

namespace Mejram
{
    ///
    public class Program
    {
        ///
        public static async Task<int> Main(string[] args)
        {
            var svc = new ServiceCollection();
            svc.AddSingleton(di => new DotGraphController());
            svc.AddSingleton(di => new SerializeController());
            var appHost = AppHostBuilder.Create(svc)
                .Recognize(typeof(DotGraphController))
                .Recognize(typeof(SerializeController))
                .Recognize(typeof(MermaidController))
                .BuildAppHost()
                ;
            try
            {
                var parsedMethod = appHost.Parse(args);
                if (parsedMethod.Unrecognized.Count != 0)//Warning:
                {
                    await Console.Error.WriteLineAsync($@"Unrecognized arguments: 
    {string.Join(",", parsedMethod.Unrecognized.Select(arg => arg.Value).ToArray())}");
                    return 1;
                }
                else
                {
                    await parsedMethod.InvokeAsync(Console.Out);
                    return 0;
                }
            }
            catch (TypeConversionFailedException ex)
            {
                await Console.Error.WriteLineAsync(
                    $"Could not convert argument {ex.Argument} with value {ex.Value} to type {ex.TargetType}");
                if (null != ex.InnerException)
                {
                    await Console.Error.WriteLineAsync("Inner exception: ");
                    await Console.Error.WriteLineAsync(ex.InnerException.Message);
                }
                return 9;
            }
            catch (MissingArgumentException ex)
            {
                await Console.Out.WriteLineAsync($"Missing argument(s): {string.Join(", ", ex.Arguments).ToArray()}");
                await Console.Out.WriteLineAsync(await appHost.HelpAsync());
                return 10;
            }
#if DEBUG
            catch (Exception ex1)
            {
                Console.WriteLine(string.Join(Environment.NewLine, new object[]
                        {
                                "The invokation failed with exception:",
                                ex1.Message, ex1.StackTrace,
                    ex1.InnerException?.Message, ex1.InnerException?.StackTrace
                        }));
                return 1;
            }
#endif

        }
    }
}