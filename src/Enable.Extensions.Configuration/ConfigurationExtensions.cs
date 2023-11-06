using Microsoft.Extensions.Configuration;

namespace Enable.Extensions.Configuration;

public static class ConfigurationExtensions
{
    private const string BranchFileName = "dev-branch.txt";
    private const string DatabaseServerFileName = "dev-database-server.txt";

    private const string FileNotFoundExceptionMessage = "DEVELOPMENT: A {0} file was not found in your apps's file path. Please create a {0} file containing the development branch name (e.g. \"2023-BS5\") in your source repository directory.";

    /// <summary>
    /// Use during development to apply application configuration value overrides based on special placeholders.
    /// For example, placeholders for branch specific or machine specific values can be fixed within application
    /// configuration files and replaced at development time with environment specific values.
    /// These values are typically not suitable for being stored in the version control repository alongside the
    /// source code.
    /// </summary>
    public static void ApplyDevelopmentOverrides(this IConfiguration configuration, SubstitutionFlags flags = SubstitutionFlags.TextFiles | SubstitutionFlags.ThrowIfTextFileNotFound)
    {
        var substitution = new ConfigurationValueSubstitution(
            branch: new Lazy<string>(() => GetBranch(flags).ReplaceSpaces()),
            branchPath: new Lazy<string>(() => GetBranchPath(flags).ReplaceSpaces()),
            databaseServer: new Lazy<string>(() => GetDatabaseServer(flags).ReplaceSpaces()),
            machineName: Environment.MachineName.ToLower().ReplaceSpaces(),
            serviceBusSharedAccessKey: Environment.GetEnvironmentVariable(EnvironmentVariableNames.ServiceBusSharedAccessKey) ?? string.Empty);

        foreach (var entry in configuration.AsEnumerable())
        {
            var overrideValue = substitution.Substitute(entry.Value);

            if (overrideValue != entry.Value)
            {
                configuration[entry.Key] = overrideValue;
            }
        }
    }

    private static string GetBranch(SubstitutionFlags flags)
    {
        if (flags.HasFlag(SubstitutionFlags.EnvironmentVariables))
        {
            var branch = Environment.GetEnvironmentVariable(EnvironmentVariableNames.Branch);

            if (branch is not null)
            {
                return branch;
            }
        }

        if (flags.HasFlag(SubstitutionFlags.TextFiles))
        {
            var branch = ReadDevelopmentEnvironmentValue(BranchFileName);

            if (branch is not null)
            {
                return branch;
            }
            else if (flags.HasFlag(SubstitutionFlags.ThrowIfTextFileNotFound))
            {
                throw new FileNotFoundException(string.Format(FileNotFoundExceptionMessage, BranchFileName), BranchFileName);
            }
        }

        return string.Empty;
    }

    private static string GetBranchPath(SubstitutionFlags flags)
    {
        if (flags.HasFlag(SubstitutionFlags.EnvironmentVariables))
        {
            var branchPath = Environment.GetEnvironmentVariable(EnvironmentVariableNames.BranchPath);

            if (branchPath is not null)
            {
                return branchPath;
            }
        }

        if (flags.HasFlag(SubstitutionFlags.TextFiles))
        {
            var branchFile = GetDevelopmentEnvironmentFile(BranchFileName);

            if (branchFile is not null)
            {
                return branchFile.DirectoryName;
            }
            else if (flags.HasFlag(SubstitutionFlags.ThrowIfTextFileNotFound))
            {
                throw new FileNotFoundException(string.Format(FileNotFoundExceptionMessage, BranchFileName), BranchFileName);
            }
        }

        return string.Empty;
    }

    private static string GetDatabaseServer(SubstitutionFlags flags)
    {
        if (flags.HasFlag(SubstitutionFlags.EnvironmentVariables))
        {
            var databaseServer = Environment.GetEnvironmentVariable(EnvironmentVariableNames.DatabaseServer);

            if (databaseServer is not null)
            {
                return databaseServer;
            }
        }

        if (flags.HasFlag(SubstitutionFlags.TextFiles))
        {
            var databaseServer = ReadDevelopmentEnvironmentValue(DatabaseServerFileName);

            if (databaseServer is not null)
            {
                return databaseServer;
            }
            else if (flags.HasFlag(SubstitutionFlags.ThrowIfTextFileNotFound))
            {
                throw new FileNotFoundException(string.Format(FileNotFoundExceptionMessage, DatabaseServerFileName), DatabaseServerFileName);
            }
        }

        return string.Empty;
    }

    private static string? ReadDevelopmentEnvironmentValue(string fileName)
    {
        var file = GetDevelopmentEnvironmentFile(fileName);
        return file is not null ? File.ReadAllText(file.FullName) : null;
    }

    private static FileInfo? GetDevelopmentEnvironmentFile(string fileName)
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(
                new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath));

        do
        {
            var filePath = Path.Combine(directory.FullName, fileName);

            if (File.Exists(filePath))
            {
                return new FileInfo(filePath);
            }

            directory = directory.Parent;
        }
        while (directory is not null);

        return null;
    }
}
