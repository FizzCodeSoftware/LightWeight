namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class ChangeIdentifierHelper
    {
        public static string ChangeIdentifier(string identifier, string newIdentifier, char startEscapeChar, Regex regex, ISqlEngineSemanticFormatter formatter)
        {
            if (identifier.Contains(startEscapeChar, StringComparison.InvariantCultureIgnoreCase))
            {
                if (identifier.Contains('.', StringComparison.InvariantCultureIgnoreCase))
                {
                    var sb = new StringBuilder();
                    var matches = regex.Matches(identifier);
                    for (var i = 0; i < matches.Count - 1; i++)
                    {
                        sb.Append(matches[i].Value);
                        sb.Append('.');
                    }

                    sb.Append(formatter.Escape(newIdentifier));
                    return sb.ToString();
                }
                else
                {
                    return formatter.Escape(newIdentifier);
                }
            }

            if (identifier.Contains('.', StringComparison.InvariantCultureIgnoreCase))
            {
                var sb = new StringBuilder();
                var groups = identifier.Split('.');
                for (var i = 0; i < groups.Length - 1; i++)
                {
                    sb.Append(groups[i]);
                    sb.Append('.');
                }

                if (formatter.IsEscaped(groups[^1]))
                    sb.Append(formatter.Escape(newIdentifier));
                else
                    sb.Append(newIdentifier);

                return sb.ToString();
            }
            else
            {
                return newIdentifier;
            }
        }
    }
}