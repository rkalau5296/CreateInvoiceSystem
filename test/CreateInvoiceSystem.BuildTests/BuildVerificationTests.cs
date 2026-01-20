namespace CreateInvoiceSystem.BuildTests;

using System.Diagnostics;

public class BuildVerificationTests
{
    [Fact]
    public void AllProjects_Should_Build_Successfully()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var src = Path.Combine(root, "src");
        
        var allCsproj = Directory.GetFiles(src, "*.csproj", SearchOption.AllDirectories);

        foreach (var project in allCsproj)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{project}\" --nologo",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = root
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(process.ExitCode == 0, $"Build failed for {project}:\n{output}\n{error}");
        }
    }
}