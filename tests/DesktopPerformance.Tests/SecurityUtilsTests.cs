using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using DesktopPerformance.Models;
using DesktopPerformance.Services;
using Xunit;

namespace DesktopPerformance.Tests;

public class SecurityUtilsTests : IDisposable
{
    private readonly string _tempDir;

    public SecurityUtilsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ZL98_SecurityTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch { }
    }

    [Fact]
    public void ComputeSha256_CalculatesKnownDeterministicHash()
    {
        string sampleFile = Path.Combine(_tempDir, "sample.txt");
        File.WriteAllText(sampleFile, "ZeroLatency98 Security Verification", Encoding.UTF8);

        string hash = SecurityUtils.ComputeSha256(sampleFile);

        Assert.NotNull(hash);
        Assert.Equal(64, hash.Length); // 64 hex characters for SHA-256
        Assert.Matches("^[a-f0-9]{64}$", hash);

        // Verify repeatability
        Assert.True(SecurityUtils.VerifyFileHash(sampleFile, hash));
        Assert.False(SecurityUtils.VerifyFileHash(sampleFile, "0000000000000000000000000000000000000000000000000000000000000000"));
    }

    [Fact]
    public void SafeExtractToDirectory_ExtractsLegitimateArchiveCleanly()
    {
        string zipPath = Path.Combine(_tempDir, "valid.zip");
        string extractDest = Path.Combine(_tempDir, "extracted");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("subfolder/hello.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("Hello Retro World");
        }

        SecurityUtils.SafeExtractToDirectory(zipPath, extractDest, overwrite: true);

        string extractedFile = Path.Combine(extractDest, "subfolder", "hello.txt");
        Assert.True(File.Exists(extractedFile));
        Assert.Equal("Hello Retro World", File.ReadAllText(extractedFile));
    }

    [Fact]
    public void SafeExtractToDirectory_BlocksZipSlipPathTraversal()
    {
        string zipPath = Path.Combine(_tempDir, "malicious_slip.zip");
        string extractDest = Path.Combine(_tempDir, "safe_target");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            // Crafted entry attempting to escape safe_target
            var entry = zip.CreateEntry("../../escaped_file.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("Dangerous payload");
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            SecurityUtils.SafeExtractToDirectory(zipPath, extractDest, overwrite: true);
        });

        Assert.Contains("Zip Slip", ex.Message);
    }

    [Theory]
    [InlineData("https://github.com/dremin/RetroBar/releases", true)]
    [InlineData("https://github.com/Open-Shell/Open-Shell-Menu/releases", true)]
    [InlineData("http://insecure-site.com/download", false)] // Non-HTTPS rejected
    [InlineData("file:///C:/Windows/System32/cmd.exe", false)] // File scheme rejected
    [InlineData("cmd://execute?param=1", false)] // Custom protocol rejected
    [InlineData("javascript:alert(1)", false)] // Javascript rejected
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidHttpsUrl_EnforcesHttpsAndRejectsInsecureSchemes(string? url, bool expectedValid)
    {
        bool isValid = SecurityUtils.IsValidHttpsUrl(url, out var uri);
        Assert.Equal(expectedValid, isValid);
        if (expectedValid)
        {
            Assert.NotNull(uri);
            Assert.Equal("https", uri!.Scheme);
        }
    }

    [Theory]
    [InlineData("cs2.exe", "cs2")]
    [InlineData("C:\\Games\\Steam\\steam.exe", "steam")]
    [InlineData("..\\..\\malicious.bat", "malicious.bat")]
    [InlineData("Discord", "Discord")]
    [InlineData("Bad!Name#With$Symbols", "BadNameWithSymbols")]
    public void SanitizeProcessName_StripsPathsAndDangerousCharacters(string input, string expected)
    {
        string result = SecurityUtils.SanitizeProcessName(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExportManager_ImportProfiles_RejectsOversizedPayloads()
    {
        var manager = new ExportManager();
        string oversizedFile = Path.Combine(_tempDir, "large_profiles.json");

        // Create a dummy file exceeding 512 KB
        using (var fs = new FileStream(oversizedFile, FileMode.Create, FileAccess.Write))
        {
            byte[] chunk = new byte[64 * 1024];
            for (int i = 0; i < 10; i++) // 640 KB
            {
                fs.Write(chunk, 0, chunk.Length);
            }
        }

        var result = manager.ImportProfiles(oversizedFile);
        Assert.Null(result); // Must be rejected due to size bound
    }

    [Fact]
    public void VerifyPinnedHashStrict_ValidatesKnownHashesAndFailsClosed()
    {
        string dummyFile = Path.Combine(_tempDir, "dummy_unpinned.bin");
        File.WriteAllText(dummyFile, "Random unverified binary payload");

        // Unpinned file must fail closed
        bool verifiedUnpinned = SecurityUtils.VerifyPinnedHashStrict(dummyFile, "unknown_payload.exe", out var expHash, out var actHash);
        Assert.False(verifiedUnpinned);
        Assert.Null(expHash);
        Assert.NotNull(actHash);

        // Tampered file with known name must fail
        string tamperedRetroBar = Path.Combine(_tempDir, "RetroBar.Portable.zip");
        File.WriteAllText(tamperedRetroBar, "Tampered fake zip bytes");
        bool verifiedTampered = SecurityUtils.VerifyPinnedHashStrict(tamperedRetroBar, "RetroBar.Portable.zip", out var expRetroHash, out var actRetroHash);
        Assert.False(verifiedTampered);
        Assert.NotNull(expRetroHash);
        Assert.NotEqual(expRetroHash, actRetroHash);
    }

    [Fact]
    public void ExportManager_ImportProfiles_CapsMassiveProfileLists()
    {
        var manager = new ExportManager();
        string massiveListFile = Path.Combine(_tempDir, "massive_profiles.json");

        var list = new List<SmartProfile>();
        for (int i = 0; i < 150; i++)
        {
            list.Add(new SmartProfile
            {
                Name = $"Profile_{i}",
                Description = $"Description for profile {i}",
                TriggerProcessName = "game.exe"
            });
        }

        manager.ExportProfiles(list, massiveListFile);

        var imported = manager.ImportProfiles(massiveListFile);
        Assert.NotNull(imported);
        Assert.True(imported.Count <= 100, $"Expected maximum 100 profiles, but got {imported.Count}");
    }

    [Fact]
    public void ProcessSupervisor_IsGenuineSystemProcess_DetectsSystemVsNonSystemProcesses()
    {
        // 1. The test runner process is in .NET SDK / program files, NOT in Windows/System32
        using var currentProc = Process.GetCurrentProcess();
        bool isRunnerSystem = ProcessSupervisor.IsGenuineSystemProcess(currentProc);
        Assert.False(isRunnerSystem, "Test runner outside System32 must NOT be classified as genuine system process");

        // 2. Windows Explorer (if running) is in C:\Windows, so it must be classified as genuine
        var explorers = Process.GetProcessesByName("explorer");
        if (explorers.Length > 0)
        {
            using var explorer = explorers[0];
            bool isExplorerSystem = ProcessSupervisor.IsGenuineSystemProcess(explorer);
            Assert.True(isExplorerSystem, "explorer.exe inside Windows directory must be classified as genuine");
        }
    }
}
