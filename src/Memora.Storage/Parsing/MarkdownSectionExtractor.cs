namespace Memora.Storage.Parsing;

internal static class MarkdownSectionExtractor
{
    public static IReadOnlyDictionary<string, string> Extract(string body)
    {
        var sections = new Dictionary<string, string>(StringComparer.Ordinal);
        var normalized = body.Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n');
        string? currentHeading = null;
        var currentContent = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush(sections, currentHeading, currentContent);
                currentHeading = line[3..].Trim();
                currentContent.Clear();
                continue;
            }

            if (currentHeading is not null)
            {
                currentContent.Add(line);
            }
        }

        Flush(sections, currentHeading, currentContent);
        return sections;
    }

    private static void Flush(IDictionary<string, string> sections, string? heading, IReadOnlyCollection<string> content)
    {
        if (heading is null)
        {
            return;
        }

        sections[heading] = string.Join('\n', content).Trim();
    }
}
