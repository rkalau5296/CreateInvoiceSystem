using System.Text.RegularExpressions;
using FluentAssertions;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class RepositoryUnitOfWorkGuardTests
{    
    private static readonly string[] AllowedFiles = ["KsefOutboxRepository.cs"];

    private static readonly Regex SaveChangesCall = new(@"\bSaveChanges(Async)?\s*\(", RegexOptions.Compiled);

    [Fact]
    public void Repositories_ShouldNotCallSaveChanges_ExceptAllowedFiles()
    {
        var repositoriesDir = Path.Combine(
            FindRepositoryRoot(), "src", "CreateInvoiceSystem.API", "Repositories");

        Directory.Exists(repositoriesDir).Should().BeTrue(
            $"katalog repozytoriów powinien istnieć: {repositoriesDir}");

        var violations = Directory
            .EnumerateFiles(repositoriesDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => !AllowedFiles.Contains(Path.GetFileName(file)))
            .SelectMany(file => File.ReadAllLines(file)
                .Select((text, index) => (File: file, Line: index + 1, Text: text.Trim())))
            .Where(x => !x.Text.StartsWith("//") && SaveChangesCall.IsMatch(x.Text))
            .Select(x => $"{Path.GetRelativePath(repositoriesDir, x.File)}:{x.Line}: {x.Text}")
            .ToList();

        violations.Should().BeEmpty(
            "repozytoria nie powinny wołać SaveChanges - zapis robi TransactionBehavior lub wołający poza MediatR. " +
            "Jeśli wyjątek jest uzasadniony, dodaj plik do AllowedFiles z komentarzem dlaczego.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException(
                "Nie znaleziono katalogu głównego repozytorium (brak Directory.Packages.props w katalogach nadrzędnych).");
    }
}