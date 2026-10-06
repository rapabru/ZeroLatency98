using System.Diagnostics;
using System.ServiceProcess;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class ServiceSupervisor
{
    private static readonly (string Name, string DisplayName, string Description)[] TargetServices = new[]
    {
        ("WSearch", "Windows Search Indexer", "Indexes files, emails and content on NTFS drives; pauses disk I/O in low-interference mode."),
        ("Corsair.Service", "Corsair iCUE Service", "RGB lighting and USB polling service; high timer resolution wakeups."),
        ("Razer Synapse Service", "Razer Synapse Service", "Peripheral management and RGB service; active background polling."),
        ("ArmouryCrate.Service", "ASUS Armoury Crate Service", "Hardware telemetry and lighting service; frequent WMI polling."),
        ("LightingService", "ASUS Aura Lighting Service", "RGB lighting driver interface; continuous polling."),
        ("AdobeARMservice", "Adobe Acrobat Update Service", "Automatic updater; background periodic checks.")
    };

    public List<ServiceInfo> GetManagedServices()
    {
        var list = new List<ServiceInfo>();

        foreach (var target in TargetServices)
        {
            try
            {
                using var sc = new ServiceController(target.Name);
                list.Add(new ServiceInfo
                {
                    ServiceName = target.Name,
                    DisplayName = target.DisplayName,
                    Description = target.Description,
                    Status = sc.Status.ToString(),
                    StartType = sc.StartType.ToString()
                });
            }
            catch
            {
                // Service is not installed on this machine
            }
        }

        return list;
    }

    public bool PauseOrStopService(string serviceName, out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                if (sc.CanPauseAndContinue)
                {
                    sc.Pause();
                    sc.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(3));
                    return true;
                }
                else if (sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(3));
                    return true;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Cannot pause service '{serviceName}': {ex.Message} (Admin rights may be required).";
            Debug.WriteLine(errorMessage);
            return false;
        }
    }

    public bool ResumeOrStartService(string serviceName, out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Paused)
            {
                sc.Continue();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(3));
                return true;
            }
            else if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(3));
                return true;
            }
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Cannot resume service '{serviceName}': {ex.Message}";
            Debug.WriteLine(errorMessage);
            return false;
        }
    }
}
