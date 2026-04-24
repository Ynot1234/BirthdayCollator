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

        //people = [.. people.Where(p => string.Equals(p.Name, "?", StringComparison.OrdinalIgnoreCase))];


        people = deduper.Deduplicate(people);
        people = cleaner.CleanPersons(people);
        people = await enricher.EnrichOnThisDayUrlsAsync(people, token);
        people = await filter.FilterLivingAsync(people, token);
        people = nearDupes.RemoveNearDuplicates(people);
        await aiEnricher.EnrichAndFilterPeopleAsync(people);
        people = sorter.SortPersons(people);
        return people;
    }
}