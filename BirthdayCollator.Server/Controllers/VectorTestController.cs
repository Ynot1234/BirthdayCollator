using BirthdayCollator.Server.AI.Semantic.Embeddings;
using BirthdayCollator.Server.AI.Semantic.VectorDb;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class VectorTestController(IEmbeddingService embeddings, IVectorStore vectorStore) : ControllerBase
    {
        [HttpGet("vector")]
        public async Task<IActionResult> TestVector()
        {
            var e1 = await embeddings.EmbedAsync("Tony loves pizza");
            var e2 = await embeddings.EmbedAsync("Tony works in software");
            var chunkId1 = Guid.NewGuid().ToString();
            var chunkId2 = Guid.NewGuid().ToString();
           
            await vectorStore.UpsertAsync("p1", chunkId1, "Tony loves pizza", e1);
            await vectorStore.UpsertAsync("p1", chunkId2, "Tony works in software", e2);

            var query = await embeddings.EmbedAsync("food and restaurants");
            var results = await vectorStore.SearchAsync("p1", query, 5);

            return Ok(results);
        }

    }

}
