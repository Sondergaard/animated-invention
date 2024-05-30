using System.Text.RegularExpressions;

namespace EventHubTestContainer;

internal static partial class RegexPatterns
{
    [GeneratedRegex("Use connection string : \"(?<ConnectionString>.+)\"")]
    internal static partial Regex ConnectionStringPattern();

    [GeneratedRegex("Emulator Service is Successfully Up!")]
    internal static partial Regex EventHubReadyPattern();
}