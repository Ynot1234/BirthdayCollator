namespace BirthdayCollator.Server.Processing.Names;

public interface IPersonNameResolver
{
    string ExtractName(string entryText);
    void FixSwappedName(Person person);
}
public sealed class PersonNameResolver : IPersonNameResolver
{
    public string ExtractName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
         
        ReadOnlySpan<char> span = text.AsSpan();
        int idx = span.IndexOf('(');

        ReadOnlySpan<char> namePart = idx > 0
            ? span[..idx]
            : span;

        return new string(namePart.Trim().TrimEnd(','));
    }

    public void FixSwappedName(Person person)
    {
        if (person is null || string.IsNullOrWhiteSpace(person.Name))
            return;

        ReadOnlySpan<char> nameSpan = person.Name.AsSpan();
        int commaIndex = nameSpan.IndexOf(',');

        if (commaIndex != -1 && nameSpan[(commaIndex + 1)..].IndexOf(',') == -1)
        {
            ReadOnlySpan<char> last = nameSpan[..commaIndex].Trim();
            ReadOnlySpan<char> first = nameSpan[(commaIndex + 1)..].Trim();

            if (!first.IsEmpty && !last.IsEmpty)
            {
                person.Name = $"{first} {last}";
            }
        }
    }
}