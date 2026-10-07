using DesktopPerformance.UI;

namespace DesktopPerformance;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}