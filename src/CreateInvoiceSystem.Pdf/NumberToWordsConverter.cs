namespace CreateInvoiceSystem.Pdf;

public static class NumberToWordsConverter
{
    private static readonly string[] Units = { "", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć" };
    private static readonly string[] Tens = { "", "dziesięć", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt" };
    private static readonly string[] Teens = { "dziesięć", "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście" };
    private static readonly string[] Hundreds = { "", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset" };
    private static readonly string[][] Groups = {
    new[] { "złoty", "złote", "złotych" },
    new[] { "tysiąc", "tysiące", "tysięcy" },
    new[] { "milion", "miliony", "milionów" }
};

    public static string Convert(decimal value)
    {
        long gold = (long)Math.Floor(value);
        int cents = (int)((value - gold) * 100);

        string result = ConvertLong(gold) + " " + GetDeclension(gold, Groups[0]);
        if (cents > 0) result += $" i {cents}/100 gr";

        return result.Trim();
    }

    private static string ConvertLong(long n)
    {
        if (n == 0) return "zero";
        string res = "";
        int groupIdx = 0;

        while (n > 0)
        {
            int part = (int)(n % 1000);
            if (part > 0)
            {
                string partStr = ConvertPart(part);
                string groupName = groupIdx > 0 ? " " + GetDeclension(part, Groups[groupIdx]) : "";
                res = partStr + groupName + " " + res;
            }
            n /= 1000;
            groupIdx++;
        }
        return res.Trim();
    }

    private static string ConvertPart(int n)
    {
        string res = Hundreds[n / 100];
        int rest = n % 100;
        if (rest >= 10 && rest < 20) res += " " + Teens[rest - 10];
        else res += " " + Tens[rest / 10] + " " + Units[rest % 10];
        return res.Trim();
    }

    private static string GetDeclension(long n, string[] forms)
    {
        if (n == 1) return forms[0];
        if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) return forms[1];
        return forms[2];
    }
}
