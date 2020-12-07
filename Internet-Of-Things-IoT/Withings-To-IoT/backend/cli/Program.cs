// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Cli
{
    using System.CommandLine;
    using System.Threading.Tasks;
    using H3.Cli.Commands;
    using H3.Core.Domain;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// This class is the entrypoint to the H3 command line utility.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var s = new ServiceCollection()
                .ConfigureH3()
                .BuildServiceProvider();

            var command = new RootCommand
            {
                new GenerateSwagger(s.GetRequiredService<ISwaggerGenerator>()).Command,
            };

            await command.InvokeAsync(args);
        }
    }
}
