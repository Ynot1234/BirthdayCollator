using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Cleaning;
using BirthdayCollator.Server.Processing.Deduplication;
using BirthdayCollator.Server.Processing.Enrichment;
using BirthdayCollator.Server.Processing.Sorting;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class PersonPipeline(
    PersonDeduper deduper,
    PersonCleaner cleaner,
    PersonFilter filter,
    PersonSorter sorter,
    NearDuplicateRemover nearDupes,
    PersonWikiEnricher enricher
   // PersonAIEnricher aiEnricher
) : IPersonPipeline
{
    public async Task<List<Person>> Process(List<Person> people, CancellationToken token)
    {
        people = await enricher.EnrichOnThisDayUrlsAsync(people, token);
        people = deduper.Deduplicate(people);
        people = cleaner.CleanPersons(people);
        people = await filter.FilterLivingAsync(people, token);
        people = sorter.SortPersons(people);
        people = nearDupes.RemoveNearDuplicates(people);
        //  people = await aiEnricher.EnrichPeopleAsync(people, token);
       return people;
    }
}