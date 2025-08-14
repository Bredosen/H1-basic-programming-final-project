using System.Reflection;

namespace H1_basic_programming_final_project.Core.Manager;
public static class AsciiManager
{
    #region Properties
    private static readonly Dictionary<string, string[]> Cache = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Load Ascii Art
    public static string[]? Load(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (Cache.TryGetValue(name, out string[]? lines))
        {
            return lines;
        }

        // 1) Content file copied to output: <bin>/Resources/<name>.txt
        string file = name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ? name : name + ".txt";
        string path = Path.Combine(AppContext.BaseDirectory, "Resources", file);
        if (File.Exists(path))
        {
            return Cache[name] = File.ReadAllLines(path);
        }

        // 2) Embedded resource fallback
        Assembly asm = Assembly.GetExecutingAssembly();
        string? res = asm.GetManifestResourceNames()
            .FirstOrDefault(n =>
                n.EndsWith($".Resources.{file}", StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith(file, StringComparison.OrdinalIgnoreCase));

        if (res == null)
        {
            return null;
        }

        using Stream? s = asm.GetManifestResourceStream(res);
        if (s == null)
        {
            return null;
        }

        using StreamReader sr = new(s);
        List<string> list = [];
        while (!sr.EndOfStream)
        {
            list.Add(sr.ReadLine() ?? string.Empty);
        }

        return Cache[name] = list.ToArray();
    }
    #endregion

    #region GetResourceFiles
    public static string[] GetResourceFiles(string searchPattern = "*.txt")
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "Resources");
        if (!Directory.Exists(dir))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(dir, searchPattern, SearchOption.TopDirectoryOnly);
    }
    #endregion

    #region PreloadAllFromResources
    /// <summary>
    /// Loads every matching file from <base>/Resources into the cache.
    /// Adds entries under both "name" and "name.txt" keys.
    /// Returns the number of files added or refreshed.
    /// </summary>
    public static int PreloadAllFromResources(string searchPattern = "*.txt")
    {
        int added = 0;
        foreach (string path in GetResourceFiles(searchPattern))
        {
            string[] lines = File.ReadAllLines(path);
            string stem = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            Cache[stem] = lines;      // e.g., "Yoda"
            Cache[file] = lines;      // e.g., "Yoda.txt"
            added++;
        }
        return added;
    }
    #endregion

    #region Get Random
    public static string GetRandom()
    {
        return Cache.Count > 0 ? Cache.Keys.ElementAt(new Random().Next(Cache.Count)) : string.Empty;
    }
    #endregion
}
