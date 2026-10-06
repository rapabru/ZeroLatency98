namespace DesktopPerformance.Models;

public enum ProfileType
{
    Normal,
    LowInterference,
    MaxResponse
}

public enum AppActionPolicy
{
    Default,
    Pause,
    LowerPriorityAndAffinity,
    NeverTouch
}

public enum ProcessExecutionState
{
    NotRunning,
    RunningNormal,
    BackgroundPriority,
    Suspended,
    Whitelisted
}
