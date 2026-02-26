using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Pipelines
{
    public interface IFetchPipeline
    {
        Task<List<Person>> FetchAllAsync(DateTime date, CancellationToken token);
        void ForceSuffixes(params string[] suffixes); 
        void ResetSuffixes(); 
    }
}
