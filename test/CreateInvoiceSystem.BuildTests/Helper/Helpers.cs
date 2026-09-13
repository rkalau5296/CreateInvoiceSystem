using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.BuildTests.Helper
{
    public static class Helpers
    {
        public static string CreateTestNip()
        {
            while (true)
            {
                var digits = Guid.NewGuid()
                    .ToString("N")
                    .Where(char.IsDigit)
                    .Take(9)
                    .ToArray();

                if (digits.Length < 9)
                    continue;

                var weights = new[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };

                var checksum = digits
                    .Select((digit, index) => (digit - '0') * weights[index])
                    .Sum() % 11;

                if (checksum == 10)
                    continue;

                return new string(digits) + checksum;
            }
        }
    }
}
