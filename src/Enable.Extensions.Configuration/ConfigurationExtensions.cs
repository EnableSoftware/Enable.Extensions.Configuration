using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Enable.Extensions.Configuration;

public static class ConfigurationExtensions
{
    private const string BranchFileName = "dev-branch.txt";
    private const string DatabaseServerFileName = "dev-database-server.txt";

    private static readonly Lazy<string> Branch = new(valueFactory: ReadBranch);
    private static readonly Lazy<string> BranchPath = new(valueFactory: ReadBranchPath);
    private static readonly Lazy<string> DatabaseServer = new(valueFactory: ReadDatabaseServer);

    /// <summary>
    /// Use during development to apply application configuration value overrides based on special placeholders.
    /// For example, placeholders for branch specific or machine specific values can be fixed within application
    /// configuration files and replaced at development time with environment specific values.
    /// These values are typically not suitable for being stored in the version control repository alongside the
    /// source code.
    /// </summary>
    public static void ApplyDevelopmentOverrides(this IConfiguration configuration)
    {
        var substitution = new ConfigurationValueSubstitution(
            Branch.Value,
            BranchPath.Value,
            DatabaseServer.Value,
            Environment.MachineName);

        foreach (var entry in configuration.AsEnumerable())
        {
            var overrideValue = substitution.Substitute(entry.Value);

            if (overrideValue != entry.Value)
            {
                configuration[entry.Key] = overrideValue;
            }
        }
    }

    private static string ReadBranch()
    {
        var branchName = ReadDevelopmentEnvironmentValue(BranchFileName);

        if (branchName is null)
        {
            throw new FileNotFoundException(
                $"DEVELOPMENT: A {BranchFileName} file was not found in your apps's file path. Please create a {BranchFileName} file containing the development branch name (e.g. \"2023-BS5\") in your source repository directory.",
                BranchFileName);
        }

        return branchName;
    }

    private static string ReadBranchPath()
    {
        var branchFileInfo = GetDevelopmentEnvironmentFileInfo(BranchFileName);

        if (branchFileInfo is null)
        {
            throw new FileNotFoundException(
                $"DEVELOPMENT: A {BranchFileName} file was not found in your apps's file path. Please create a {BranchFileName} file containing the development branch name (e.g. \"2023-BS5\") in your source repository directory.",
                BranchFileName);
        }

        return branchFileInfo.DirectoryName;
    }

    private static string ReadDatabaseServer()
    {
        var databaseServer = ReadDevelopmentEnvironmentValue(DatabaseServerFileName);

        if (databaseServer is null)
        {
            throw new FileNotFoundException(
                $"DEVELOPMENT: A {DatabaseServerFileName} file was not found in your app's file path. Please create a {DatabaseServerFileName} file containing a database server name or IP address (e.g. \"(local)\") in your source repository directory.",
                BranchFileName);
        }

        return databaseServer;
    }

    private static string? ReadDevelopmentEnvironmentValue(string fileName)
    {
        var file = GetDevelopmentEnvironmentFileInfo(fileName);

        return file is not null ? File.ReadAllText(file.FullName) : null;
    }

    private static FileInfo? GetDevelopmentEnvironmentFileInfo(string fileName)
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(
                new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath));

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
