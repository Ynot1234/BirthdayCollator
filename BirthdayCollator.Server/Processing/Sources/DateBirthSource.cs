using BirthdayCollator.Helpers;
using BirthdayCollator.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class DateBirthSource(WikiHtmlFetcher fetcher, IYearRangeProvider yearRangeProvider, IPersonNameResolver nameResolver) : IBirthSource
{
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        IReadOnlySet<string> yearSet = _yearRangeProvider.GetYearSet();

        BirthEntryValidator validator = new([.. yearSet], RegexPatterns.ExcludeDiedRegex());
        PersonFactory factory = new(WikiUrlBuilder.NormalizeWikiHref, nameResolver);
        DatePageParser parser = new(validator, factory);

        string pageName = $"{actualDate:MMMM}_{actualDate.Day}";

        try
        {
            token.ThrowIfCancellationRequested();
            string html = await fetcher.FetchHtmlAsync(pageName, token);
            token.ThrowIfCancellationRequested();

            return parser.Parse(html, actualDate.Month, actualDate.Day);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }
}