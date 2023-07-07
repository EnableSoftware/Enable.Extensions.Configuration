using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Enable.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        private const string BranchFileName = "dev-branch.txt";
        private const string DatabaseServerFileName = "dev-database-server.txt";

        private static readonly Lazy<string> Branch = new(InitBranch);
        private static readonly Lazy<string> BranchPath = new(InitBranchPath);
        private static readonly Lazy<string> DatabaseServer = new(InitDatabaseServer);
        private static readonly ServiceBusConnectionStringSubstitution _serviceBusConnectionStringSubstitution = new(Environment.MachineName);

        /// <summary>
        /// Use during development to apply application configuration value overrides based on special placeholders.
        /// For example, placeholders for branch specific or machine specific values can be fixed within application
        /// configuration files and replaced at development time with environment specific values.
        /// These values are typically not suitable for being stored in the version control repository alongside the
        /// source code.
        /// </summary>
        public static void ApplyDevelopmentOverrides(this IConfiguration configuration)
        {
            foreach (var entry in configuration.AsEnumerable())
            {
                var overrideValue = ReplacePlaceholderValues(entry.Value);

                if (overrideValue != entry.Value)
                {
                    configuration[entry.Key] = overrideValue;
                }
            }
        }

        private static string? ReplacePlaceholderValues(string settingValue, bool replaceSpaceWithUnderscore = true)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                return settingValue;
            }

            var branch = Branch.Value;
            var branchPath = BranchPath.Value;
            var databaseServer = DatabaseServer.Value;
            var machineName = Environment.MachineName;

            if (replaceSpaceWithUnderscore)
            {
                branch = branch.Replace(' ', '_');
                branchPath = branchPath.Replace(' ', '_');
                databaseServer = databaseServer.Replace(' ', '_');
                machineName = machineName.Replace(' ', '_');
            }

            settingValue = settingValue.Replace("{Branch}", branch);
            settingValue = settingValue.Replace("{BranchPath}", branchPath);
            settingValue = settingValue.Replace("{DatabaseServer}", databaseServer);
            settingValue = settingValue.Replace("{MachineName}", machineName);

            settingValue = _serviceBusConnectionStringSubstitution.Substitute(settingValue);

            return settingValue;
        }

        private static string InitBranch()
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

        private static string InitBranchPath()
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

        private static string InitDatabaseServer()
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
}
