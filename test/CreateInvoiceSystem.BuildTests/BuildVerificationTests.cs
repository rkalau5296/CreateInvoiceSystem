namespace CreateInvoiceSystem.BuildTests;

using System.Diagnostics;

public class BuildVerificationTests
{
    [Fact]
    public void AllProjects_Should_Build_Successfully()
    {
        var projectsToBuild = new[]
        {
            @"..\..\..\..\..\src\CreateInvoiceSystem.Abstractions\CreateInvoiceSystem.Abstractions.csproj",            
            @"..\..\..\..\..\src\CreateInvoiceSystem.API\CreateInvoiceSystem.API.csproj",
            @"..\..\..\..\..\src\CreateInvoiceSystem.Modules.Clients\CreateInvoiceSystem.Modules.Clients.csproj",
            @"..\..\..\..\..\src\CreateInvoiceSystem.Identity\CreateInvoiceSystem.Identity.csproj",            
            @"..\..\..\..\..\src\CreateInvoiceSystem.Invoices\CreateInvoiceSystem.Invoices.csproj",            
            @"..\..\..\..\..\src\CreateInvoiceSystem.Nbp\CreateInvoiceSystem.Nbp.csproj",
            @"..\..\..\..\..\src\CreateInvoiceSystem.Persistence\CreateInvoiceSystem.Persistence.csproj",
            @"..\..\..\..\..\src\CreateInvoiceSystem.Products\CreateInvoiceSystem.Products.csproj",            
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