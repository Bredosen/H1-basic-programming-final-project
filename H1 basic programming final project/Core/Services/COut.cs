namespace H1_basic_programming_final_project.Core.Services;

public static class COut
{
    #region Header
    public static void Header(int headerSize, string header)
    {
        SetColor(ConsoleColor.Cyan);
        Write(new string('=', headerSize));

        SetColor(ConsoleColor.White);
        Write($"[{header}]");

        SetColor(ConsoleColor.Cyan);
        WriteLine(new string('=', headerSize));

        ResetColor();
    }
    #endregion

    #region Space
    public static void Space(int spaceSize = 1)
    {
        for (int i = 0; i < spaceSize; i++)
        {
            Console.WriteLine();
        }
    }
    #endregion

    #region Write
    public static void Write(params object[] arguments)
    {
        foreach (object argument in arguments)
        {
            Console.Write(argument);
        }
    }
    #endregion

    #region Set Color
    public static void SetColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }
    #endregion

    #region Reset Color
    public static void ResetColor()
    {
        Console.ResetColor();
    }
    #endregion

    #region Write Line
    public static void WriteLine(params object[] arguments)
    {
        foreach (object argument in arguments)
        {
            Console.WriteLine(argument?.ToString() ?? string.Empty);
        }
    }
    #endregion

    #region Write List
    public static void WriteList<T>(List<T> list, bool printIndex = true)
    {
        foreach (T? item in list)
        {
            if (printIndex)
            {
                SetColor(ConsoleColor.Magenta);
                Write($"({list.IndexOf(item)})");
                ResetColor();
                SetColor(ConsoleColor.Green);
                Console.WriteLine($" > {item}");
                ResetColor();
                continue;
            }
            SetColor(ConsoleColor.Green);
            Console.WriteLine($"{item}");
            ResetColor();
        }
    }
    #endregion

    #region Clear
    public static void Clear()
    {
        Console.Clear();
    }
    #endregion

    #region Wait For Continue
    public static void WaitForContinue()
    {
        SetColor(ConsoleColor.Yellow);
        WriteLine("Press any key to continue...");
        ResetColor();
        _ = Console.ReadKey();
    }
    #endregion

    #region Get User Input
    public static string GetUserInput(string prompt = " >> ")
    {
        SetColor(ConsoleColor.Green);
        Write(prompt);
        ResetColor();
        if ((Console.ReadLine() ?? string.Empty) is not string input)
        {
            return string.Empty;
        }

        return input;
    }
    #endregion
}
