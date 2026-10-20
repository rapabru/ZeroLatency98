using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DesktopPerformance.Services;

/// <summary>
/// Central security utility providing cryptographic integrity verification,
/// path canonicalization, Zip Slip mitigation, and URL/process input sanitization.
/// </summary>
public static partial class SecurityUtils
{
    private static readonly Regex SafeProcessNameRegex = new(@"^[a-zA-Z0-9_\-\. ]{1,64}$", RegexOptions.Compiled);

    /// <summary>
    /// Known official SHA-256 hashes for pinned companion tool releases.
    /// Used to guarantee binary integrity prior to execution.
    /// </summary>
    private static readonly Dictionary<string, string> PinnedHashes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Open-Shell v4.4.198 official setup executable
        ["OpenShellSetup_4_4_198.exe"] = "2b1efbe5591ee599b581b22e7075c3f9157cf4356ef231ec69b0fa5c4d3bc35f",
        // RetroBar v1.22.122 official portable release zip
        ["RetroBar.Portable.zip"] = "6ab0d0972535e5efedc13c83403b52883edb06881d2d6aa18bf8af379bc7a128"
    };

    /// <summary>
    /// Computes the SHA-256 hex string of a given file.
    /// </summary>
    public static string ComputeSha256(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found for hash calculation", filePath);

        using var sha256 = SHA256.Create();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
        byte[] hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies that the file at <paramref name="filePath"/> matches <paramref name="expectedHexHash"/>.
    /// </summary>
    public static bool VerifyFileHash(string filePath, string expectedHexHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHexHash) || !File.Exists(filePath))
            return false;

        string actual = ComputeSha256(filePath);
        return string.Equals(actual, expectedHexHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a known pinned hash exists for a given file name and strictly validates it.
    /// Fails closed: returns false if the hash does not match or if no pinned hash is registered (ZL98-SEC-01).
    /// </summary>
    public static bool VerifyPinnedHashStrict(string filePath, string fileName, out string? expectedHash, out string actualHash)
    {
        actualHash = ComputeSha256(filePath);
        if (PinnedHashes.TryGetValue(fileName, out expectedHash))
        {
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        // Fail-closed: unpinned binaries cannot be executed blindly
        expectedHash = null;
        return false;
    }

    /// <summary>
    /// Legacy alias delegating strictly to <see cref="VerifyPinnedHashStrict"/>.
    /// </summary>
    public static bool VerifyPinnedHashIfPresent(string filePath, string fileName, out string? expectedHash, out string actualHash)
    {
        return VerifyPinnedHashStrict(filePath, fileName, out expectedHash, out actualHash);
    }

    /// <summary>
    /// Safely extracts an archive to a destination directory, strictly guarding against Zip Slip
    /// (path traversal outside target directory) and dangerous filesystem links.
    /// </summary>
    public static void SafeExtractToDirectory(string archivePath, string destinationDirectory, bool overwrite = true)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("Archive file does not exist", archivePath);

        string destFullPath = Path.GetFullPath(destinationDirectory);
        if (!destFullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            destFullPath += Path.DirectorySeparatorChar;
        }

        Directory.CreateDirectory(destFullPath);

        using var archive = ZipFile.OpenRead(archivePath);
        foreach (var entry in archive.Entries)
        {
            // Normalize path separators
            string normalizedEntryName = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
            string entryDestPath = Path.GetFullPath(Path.Combine(destFullPath, normalizedEntryName));

            // CRITICAL: Ensure destination path stays strictly inside target directory (Anti-Zip Slip)
            if (!entryDestPath.StartsWith(destFullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Security Violation: Zip Slip path traversal attempted by archive entry: {entry.FullName}");
            }

            // If it's a directory entry
            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
            {
                Directory.CreateDirectory(entryDestPath);
            }
            else
            {
                // Ensure parent directory exists
                string? parentDir = Path.GetDirectoryName(entryDestPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // Guard against symlink / junction point overwriting (ZL98-SEC-02 / CWE-59)
                if (File.Exists(entryDestPath))
                {
                    var fileAttr = File.GetAttributes(entryDestPath);
                    if ((fileAttr & FileAttributes.ReparsePoint) != 0)
                    {
                        throw new InvalidOperationException($"Security Violation: Extraction targeting reparse point / symlink is prohibited: {entry.FullName}");
                    }
                }

                entry.ExtractToFile(entryDestPath, overwrite);
            }
        }
    }

    /// <summary>
    /// Validates that a URL is a secure, well-formed HTTPS web URL before launching via ShellExecute.
    /// Prevents arbitrary command injection and URI handler abuse (e.g. file://, cmd:, powershell:).
    /// </summary>
    public static bool IsValidHttpsUrl(string? url, out Uri? validUri)
    {
        validUri = null;
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsed))
        {
            if (parsed.Scheme == Uri.UriSchemeHttps && !parsed.IsFile && !parsed.IsUnc)
            {
                validUri = parsed;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sanitizes and validates a process or application trigger name.
    /// Strips paths and restricts characters to prevent injection.
    /// </summary>
    public static string SanitizeProcessName(string? rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return string.Empty;

        // Strip any directory paths
        string fileName = Path.GetFileName(rawName.Trim());

        if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName.Substring(0, fileName.Length - 4);
        }

        if (SafeProcessNameRegex.IsMatch(fileName))
        {
            return fileName;
        }

        // Remove any illegal chars
        var cleanChars = fileName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.').ToArray();
        return new string(cleanChars);
    }

    /// <summary>
    /// Resolves canonical system executable paths (e.g., C:\Windows\System32\rundll32.exe)
    /// to prevent binary planting (CWE-427) from current working directory.
    /// </summary>
    public static string GetSystem32Executable(string exeName)
    {
        return Path.Combine(Environment.SystemDirectory, exeName);
    }

    /// <summary>
    /// Resolves canonical Windows explorer.exe path.
    /// </summary>
    public static string GetWindowsExplorerExecutable()
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return Path.Combine(winDir, "explorer.exe");
    }
}
