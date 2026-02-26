namespace BirthdayCollator.Server.Constants
{
    public class NameParsing
    {

        public static readonly HashSet<string> Stopwords =
           new(StringComparer.OrdinalIgnoreCase)
           {
             "of", "in", "the", "and", "to", "for", "on", "at", "by", "a", "an"
           };

     
        public static readonly HashSet<string> Titles =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "sir", "dr", "mr", "mrs", "ms", "prof",
            "earl", "duke", "baron", "lord", "lady",
            "king", "queen", "prince", "princess",
            "count", "countess", "viscount", "marquess"
            };

        public static readonly HashSet<string> Prefixes = 
            new(StringComparer.OrdinalIgnoreCase)
             {
                "is a ",
                "is an ",
                "was a ",
                "was an ",
                "is the ",
                "was the "
             };
    }
}
