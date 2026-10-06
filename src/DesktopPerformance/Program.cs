using System.Windows.Forms.VisualStyles;
using DesktopPerformance.UI;

namespace DesktopPerformance;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // Disable modern UX visual styles so standard controls render with authentic classic 3D beveled Win95/98/2000 borders
        Application.VisualStyleState = VisualStyleState.NoneEnabled;
        
        Application.Run(new MainForm());
    }    
}