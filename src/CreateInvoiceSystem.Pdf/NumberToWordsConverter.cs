using System.Numerics;

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
        new[] { "milion", "miliony", "milionów" },
        new[] { "miliard", "miliardy", "miliardów" },
        new[] { "bilion", "biliony", "bilionów" },
        new[] { "biliard", "biliardy", "biliardów" },
        new[] { "trylion", "tryliony", "trylionów" }
    };

    public static string Convert(decimal value)
    {
        if (value < 0) return "minus " + Convert(decimal.Negate(value));
        decimal integerPartDecimal = decimal.Truncate(value);
        decimal fractional = decimal.Round((value - integerPartDecimal) * 100, 0, MidpointRounding.ToZero);
        int cents = (int)fractional;
        BigInteger gold = new BigInteger(integerPartDecimal);
        string result = ConvertBigInteger(gold) + " " + GetDeclension(gold, Groups[0]);
        if (cents > 0) result += $" i {cents}/100 gr";
        return result.Trim();
    }

    private static string ConvertBigInteger(BigInteger n)
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
                string groupName = groupIdx > 0 ? " " + GetDeclension(new BigInteger(part), Groups[groupIdx]) : "";
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

    private static string GetDeclension(BigInteger number, string[] forms)
    {
        int lastTwo = (int)(number % 100);
        int lastDigit = (int)(number % 10);
        if (lastTwo >= 10 && lastTwo <= 20) return forms[2];
        if (lastDigit == 1) return forms[0];
        if (lastDigit >= 2 && lastDigit <= 4) return forms[1];
        return forms[2];
    }
}