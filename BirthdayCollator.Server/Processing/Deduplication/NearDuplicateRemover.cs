using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Deduplication;

public class NearDuplicateRemover
{
    private static string NormalizeWikiUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        return url.Trim().ToLowerInvariant()
            .Replace("_,jr.", "_jr.")
            .Replace("jr.", "jr")
            .Replace(",", "");
    }
    public List<Person> RemoveNearDuplicates(List<Person> people)
    {
        List<Person> result =
        [
            .. people
                .GroupBy(p => NormalizeWikiUrl(p.Url))
                .Select(g => g
                    .OrderBy(p =>
                        p.Url?.Contains(AppStrings.Slugs.Wikipedia, StringComparison.OrdinalIgnoreCase) == true ? 0 :
                        p.Url?.Contains(AppStrings.Slugs.Imdb,      StringComparison.OrdinalIgnoreCase) == true ? 1 :
                        p.Url?.Contains(AppStrings.Slugs.Genarians, StringComparison.OrdinalIgnoreCase) == true ? 2 :
                        p.Url?.Contains(AppStrings.Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase) == true ? 3 : 4)
                    .First())
        ];

        return result;
    }
}