using System.Net;

namespace BirthdayCollator.Helpers
{

    //currently unused, but may be useful for normalizing HTML content
    public class HtmlNormalization
    {
        public static string NormalizeHtml(string html)
        {
            string decoded = WebUtility.HtmlDecode(html);
            return decoded;
        }
    }
}
