namespace FizzCode.LightWeight.MsTest
{
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertCustom
    {
        /// <summary>
        /// Compares two strings, in case of non equality, indicating the first difference in the exception message.
        /// Works like the <see cref="Assert.AreEqual(string, string, bool)"/> method.
        /// </summary>
        /// <param name="expected">The first string to compare. This is the string the tests expects.</param>
        /// <param name="actual">The second string to compare. This is the string produced by the code under test.</param>
        /// <param name="ignoreCase">A Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <exception cref="AssertFailedException">Thrown if expected is not equal to actual.</exception>
        public static void AreEqual(string expected, string actual, bool ignoreCase = false)
        {
            var areEqual = string.Compare(expected, actual, ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal);
            if (areEqual == 0)
                return;

            var expecteds = expected.Split("\r\n");
            var actuals = actual.Split("\r\n");

            var msg = new StringBuilder("Assert.AreEqual failed. First difference on line ");

            for (var i = 0; i < expecteds.Length; i++)
            {
                if (actuals.Length == i)
                {
                    msg.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                    msg.Append(". Actual has less rows (");
                    msg.Append(i - 1);
                    msg.AppendLine(") than expected:");
                    msg.AppendLine("ˇ");
                    msg.AppendLine(expecteds[i]);
                    msg.AppendLine();
                    break;
                }

                areEqual = string.Compare(expecteds[i], actuals[i], ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal);

                if (areEqual == 0)
                    continue;

                var firstDiffIndex = expecteds[i].Zip(actuals[i], (c1, c2) => c1 == c2).TakeWhile(b => b).Count();

                msg.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                msg.AppendLine(":");
                msg.Append(' ', firstDiffIndex);
                msg.AppendLine("ˇ");
                msg.AppendLine(expecteds[i]);
                msg.Append(actuals[i]);

                break;
            }

            msg.AppendLine();
            msg.AppendLine();
            msg.AppendLine("Expected followed by actual:");
            msg.AppendLine(expected);
            msg.AppendLine();
            msg.AppendLine(actual);

            throw new AssertFailedException(msg.ToString());
        }
    }
}
