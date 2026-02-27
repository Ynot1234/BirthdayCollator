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
    DeduplicateByURL urlDeduper,
    PersonDedupe descriptionDupes,
    PersonWikiEnricher enricher
    //PersonAIEnricher aiEnricher
    ) : IPersonPipeline
{
    public async Task<List<Person>> Process(List<Person> people)
    {
        people = await enricher.EnrichOnThisDayUrlsAsync(people);
        people = deduper.DeduplicateByNameAndYear(people);
        people = cleaner.CleanPersons(people);
        people = filter.FilterLivingPeople(people);
        people = sorter.SortPersons(people);
        people = nearDupes.RemoveNearDuplicates(people);
        people = urlDeduper.DeduplicateByUrl(people);
        people = descriptionDupes.DedupePeople(people);
      //  people = await aiEnricher.EnrichPeopleAsync(people);
        return people;
    }
}