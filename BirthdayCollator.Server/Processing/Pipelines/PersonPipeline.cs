using BirthdayCollator.Server.Processing.Cleaning;
using BirthdayCollator.Server.Processing.Deduplication;
using BirthdayCollator.Server.Processing.Sorting;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class PersonPipeline(
    PersonDeduper deduper,
    PersonCleaner cleaner,
    PersonFilter filter,
    PersonSorter sorter,
    NearDuplicateRemover nearDupes,
    PersonWikiEnricher enricher,
    PersonAIEnricher aiEnricher
) : IPersonPipeline
{
    public async Task<List<Person>> Process(List<Person> people, CancellationToken token)
    {

       //people = [.. people.Where(p => p.Name.Contains("", StringComparison.OrdinalIgnoreCase))];

        people = deduper.Deduplicate(people);
        people = cleaner.CleanPersons(people);

        List<Person> onThisDay = [.. people.Where(p => p.SourceSlug == Slugs.OnThisDay)];
        List<Person> notOnThisDay = [.. people.Where(p => p.SourceSlug != Slugs.OnThisDay)];

        var enriched = await enricher.EnrichOnThisDayUrlsAsync(onThisDay, token);
        people = [.. enriched, .. notOnThisDay];

        List<Person> imdbPeople = [.. people.Where(p => p.Url.Contains("imdb", StringComparison.OrdinalIgnoreCase))];
        List<Person> nonImdbPeople = [.. people.Where(p => !p.Url.Contains("imdb", StringComparison.OrdinalIgnoreCase))];

        var filtered = await filter.FilterLivingAsync(nonImdbPeople, token);
        people = [.. imdbPeople, .. filtered];


        people = nearDupes.RemoveNearDuplicates(people);
        await aiEnricher.EnrichAndFilterPeopleAsync(people);
        people = sorter.SortPersons(people);
        return people;
    }
}