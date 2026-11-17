using System.Text.RegularExpressions;

namespace CreateInvoiceSystem.BuildTests.Unit
{
    public class FrontendMarkupTests
    {
        private static readonly string[] GuardedFiles =
        [
            "InvoiceCreate.razor",
            "InvoiceEdit.razor"
        ];

        [Fact]
        public void Invoice_Pages_Should_Not_Use_Raw_Input_Elements()
        {
            var repoRoot = FindRepoRoot();
            var pagesDir = Path.Combine(repoRoot, "src", "CreateInvoiceSystem.Frontend", "Pages");

            var violations = new List<string>();

            foreach (var fileName in GuardedFiles)
            {
                var path = Path.Combine(pagesDir, fileName);
                if (!File.Exists(path)) continue;

                var content = File.ReadAllText(path);
                if (Regex.IsMatch(content, @"<input(?![\w-])", RegexOptions.IgnoreCase))
                    violations.Add(fileName);
            }

            Assert.True(violations.Count == 0,
                $"Znaleziono gołe <input> zamiast komponentu Simple*Input w: {string.Join(", ", violations)}. " +
                "Pola na tych stronach powinny korzystać z komponentów w folderze Components " +
                "(SimpleTextInput, SimpleSelect, SimpleIntInput, SimpleDecimalInput, SimpleEmailInput).");
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && dir.GetFiles("*.sln").Length == 0)
                dir = dir.Parent;

            if (dir == null)
                throw new DirectoryNotFoundException("Nie znaleziono katalogu głównego repozytorium (brak pliku .sln).");

            return dir.FullName;
        }
    }
}
