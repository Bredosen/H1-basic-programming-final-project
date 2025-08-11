using System.Text;

namespace H1_basic_programming_final_project.Core.Services;

public static class COut
{
    #region Header
    public static void Header(int headerSize, string header)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append('=', headerSize);
        stringBuilder.Append($"[{header}]");
        stringBuilder.Append('=', headerSize);
        WriteLine(stringBuilder.ToString());
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
        foreach (var argument in arguments)
        {
            Console.Write(argument);
        }
    }
    #endregion

    #region Write Line
    public static void WriteLine(params object[] arguments)
    {
        foreach (var argument in arguments)
        {
            Console.WriteLine(argument);
        }
    }
    #endregion

    #region Write List
    public static void WriteList<T>(List<T> list, bool printIndex = true)
    {
        foreach (var item in list)
        {
            if (printIndex)
            {
                Console.WriteLine($"({list.IndexOf(item)}) > {item}");
                continue;
            }

            Console.WriteLine(item);
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
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
    #endregion

    #region Get User Input
    public static string GetUserInput(string prompt = " >> ")
    {
        Write(prompt);
        if ((Console.ReadLine() ?? string.Empty) is not string input)
        {
            return string.Empty;
        }

        return input;
    }
    #endregion
}
