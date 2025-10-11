namespace InvoiceCreateSystem.BuildTests;

using System.Diagnostics;

public class BuildVerificationTests
{
    [Fact]
    public void AllProjects_Should_Build_Successfully()
    {
        var projectsToBuild = new[]
        {            
            @"..\..\..\..\CreateInvoiceSystem.Address\CreateInvoiceSystem.Address.csproj",
            @"..\..\..\..\CreateInvoiceSystem.Clients\CreateInvoiceSystem.Clients.csproj",
            @"..\..\..\..\CreateInvoiceSystem.InvoicePositions\CreateInvoiceSystem.InvoicePositions.csproj",
            @"..\..\..\..\CreateInvoiceSystem.Invoices\CreateInvoiceSystem.Invoices.csproj",
            @"..\..\..\..\CreateInvoiceSystem.MethodsOfPayments\CreateInvoiceSystem.MethodsOfPayments.csproj",
            @"..\..\..\..\CreateInvoiceSystem.Persistence\CreateInvoiceSystem.Persistence.csproj",
            @"..\..\..\..\CreateInvoiceSystem.Products\CreateInvoiceSystem.Products.csproj",
            @"..\..\..\..\CreateInvoiceSystem.SharedKernel\CreateInvoiceSystem.SharedKernel.csproj",
            @"..\..\..\..\CreateInvoiceSystem.Web\CreateInvoiceSystem.Web.csproj"

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