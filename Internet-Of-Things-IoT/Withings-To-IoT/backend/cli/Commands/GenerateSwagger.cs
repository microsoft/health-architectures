// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Cli.Commands
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Text;
    using H3.Core.Domain;

    /// <summary>
    /// This class implements a command line utility which generates the Swagger specification
    /// for the H3 API and saves it to a JSON file.
    /// </summary>
    internal class GenerateSwagger
    {
        private readonly ISwaggerGenerator swaggerGenerator;

        public GenerateSwagger(ISwaggerGenerator swaggerGenerator)
        {
            this.swaggerGenerator = swaggerGenerator;
        }

        public Command Command
        {
            get
            {
                var command = new Command("swagger");

                command.AddArgument(new Argument<FileInfo>("outputFile"));

                command.Handler = CommandHandler.Create(async (FileInfo outputFile) =>
                {
                    var swagger = await swaggerGenerator.GenerateSwagger();

                    await File.WriteAllBytesAsync(outputFile.FullName, Encoding.UTF8.GetBytes(swagger));

                    return (int)Status.Ok;
                });

                return command;
            }
        }
    }
}
