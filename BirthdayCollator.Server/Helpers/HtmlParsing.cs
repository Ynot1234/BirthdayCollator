using HtmlAgilityPack;

namespace BirthdayCollator.Server.Helpers
{
    public class HtmlParsing
    {
        public static HtmlNode? SelectNode(string html, string xpath)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.SelectSingleNode(xpath);
        }
    }
}