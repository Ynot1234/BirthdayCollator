using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Deduplication
{
    public class NearDuplicateRemover
    {
        public List<Person> RemoveNearDuplicates(List<Person> people)
        {
            List<Person> result = [.. people
                                     .GroupBy(p => UrlNomalization.NormalizeWikiUrl(p.Url))
                                     .Select(g => g
                                   .OrderBy(p =>
                                        p.Url?.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase) == true ? 0 :
                                        p.Url?.Contains("genarians", StringComparison.OrdinalIgnoreCase) == true ? 1 :
                                        p.Url?.Contains("onthisday", StringComparison.OrdinalIgnoreCase) == true ? 2 : 3)
                                    .First())

                                  ];


            return result;
        }
    }
}