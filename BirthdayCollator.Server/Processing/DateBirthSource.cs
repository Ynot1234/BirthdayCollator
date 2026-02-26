using BirthdayCollator.Helpers;
using BirthdayCollator.Models;
using BirthdayCollator.Server.Processing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BirthdayCollator.Processing;

public sealed class DateBirthSource(WikiHtmlFetcher fetcher, IYearRangeProvider yearRangeProvider) : IBirthSource
{
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        IReadOnlySet<string> yearSet = _yearRangeProvider.GetYearSet();

        BirthEntryValidator validator = new([.. yearSet], RegexPatterns.ExcludeDiedRegex());
        PersonFactory factory = new(WikiUrlBuilder.NormalizeWikiHref);
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