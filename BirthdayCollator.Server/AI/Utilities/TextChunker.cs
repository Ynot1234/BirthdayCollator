namespace BirthdayCollator.Server.AI.Utilities;

public static class TextChunker
{
    public static IEnumerable<string> Chunk(string text, int chunkSize)
    {
        for (int i = 0; i < text.Length; i += chunkSize)
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
    }
}
