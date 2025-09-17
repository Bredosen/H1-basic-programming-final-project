
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Pages;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace H1_basic_programming_final_project.Core;
public sealed class H1BasicProgrammingFinalProject
{
    #region Singleton
    private static readonly Lazy<H1BasicProgrammingFinalProject> _instance = new(() => new H1BasicProgrammingFinalProject(), true);
    public static readonly H1BasicProgrammingFinalProject Instance = _instance.Value;
    #endregion

    #region Constructor
    public H1BasicProgrammingFinalProject()
    {

    }
    #endregion

    #region [Entry Point] - Main Start
    public static void Main(string[] args)
    {

         Instance.Initialize();
         Instance.Terminate();

        if (true) return;
        const int listenPort = 5000; // must match Arduino's remotePort

        using var udpClient = new UdpClient(listenPort);
        Console.WriteLine($"Listening for UDP on port {listenPort}...");

        var remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            byte[] data = udpClient.Receive(ref remoteEP);
            string message = Encoding.ASCII.GetString(data);
            Console.WriteLine($"{remoteEP.Address}: {message.Trim()}");
        }
    }
    #endregion

    #region Initialize
    public void Initialize()
    {
        _ = AsciiManager.PreloadAllFromResources();
        TaskManager.Instance.LoadTasks();
        PageManager.Instance.Initialize();
        PageManager.Instance.SetActivePage(MainMenuPage.Instance.Name);
        PageManager.Instance.Run();
    }
    #endregion

    #region Terminate
    public void Terminate()
    {
        TaskManager.Instance.SaveTasks();
        Console.WriteLine("Press any key to exit...");
        _ = Console.ReadKey();
    }
    #endregion
}
