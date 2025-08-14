namespace H1_basic_programming_final_project.Core.Manager;

using System.IO;
using System.Reflection;

static class AsciiManager
{
    private static readonly Dictionary<string, string[]> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static string[]? Load(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (Cache.TryGetValue(name, out var lines)) return lines;

        // 1) Content file copied to output: <bin>/Resource/<name>.txt
        string file = name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ? name : name + ".txt";
        string path = Path.Combine(AppContext.BaseDirectory, "Resources", file);
        if (File.Exists(path))
            return Cache[name] = File.ReadAllLines(path);

        // 2) Embedded resource fallback
        var asm = Assembly.GetExecutingAssembly();
        string? res = asm.GetManifestResourceNames()
            .FirstOrDefault(n =>
                n.EndsWith($".Resources.{file}", StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith(file, StringComparison.OrdinalIgnoreCase));

        if (res == null) return null;

        using var s = asm.GetManifestResourceStream(res);
        if (s == null) return null;
        using var sr = new StreamReader(s);
        var list = new List<string>();
        while (!sr.EndOfStream) list.Add(sr.ReadLine() ?? string.Empty);
        return Cache[name] = list.ToArray();
    }
}
