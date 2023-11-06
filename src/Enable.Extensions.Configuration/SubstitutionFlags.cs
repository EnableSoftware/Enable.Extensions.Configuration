namespace Enable.Extensions.Configuration;

[Flags]
public enum SubstitutionFlags
{
    None = 0,                         // 00000000
    TextFiles = 1 << 0,               // 00000001
    EnvironmentVariables = 1 << 1,    // 00000010
    ThrowIfTextFileNotFound = 1 << 2, // 00000100
}
