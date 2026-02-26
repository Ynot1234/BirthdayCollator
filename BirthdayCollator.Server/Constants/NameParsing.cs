namespace BirthdayCollator.Server.Constants
{
    public class NameParsing
    {

        public static readonly HashSet<string> Stopwords =
           new(StringComparer.OrdinalIgnoreCase)
           {
            "of", "the", "and", "a", "an"
           };

        public static readonly HashSet<string> Titles =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "sir", "dr", "mr", "mrs", "ms", "prof",
            "earl", "duke", "baron", "lord", "lady",
            "king", "queen", "prince", "princess",
            "count", "countess", "viscount", "marquess"
            };
    }
}
