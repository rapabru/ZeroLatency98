using System.Diagnostics;
using DesktopPerformance.UI;

namespace DesktopPerformance;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            Debug.WriteLine($"[ZeroLatency98 Unhandled Thread Exception] {e.Exception}");
            MessageBox.Show($"Ocurrió un error inesperado:\n{e.Exception.Message}", "ZeroLatency98 - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[ZeroLatency98 Unhandled Domain Exception] {e.ExceptionObject}");
        };

        Application.Run(new MainForm());
    }    
}