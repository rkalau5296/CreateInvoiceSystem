using System.Diagnostics;
using Xunit;

namespace InvoiceCreateSystem.BuildTests
{
    public class BuildVerificationTests
    {
        [Fact]
        public void AllProjects_Should_Build_Successfully()
        {
            var projectsToBuild = new[]
            {
                @"..\..\..\..\ClientService.API\ClientService.API.csproj",
                @"..\..\..\..\ClientService.Application\ClientService.Application.csproj",
                @"..\..\..\..\ClientService.Domain\ClientService.Domain.csproj",
                @"..\..\..\..\ClientService.Infrastructure\ClientService.Infrastructure.csproj",
                @"..\..\..\..\CreateInvoiceSystem.Blazor\CreateInvoiceSystem.Blazor.csproj"                
            };

            foreach (var project in projectsToBuild)
            {
                var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"build \"{project}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Assert.True(process.ExitCode == 0, $"Build failed for {project}:\n{output}\n{error}");
            }
        }

    }
}
