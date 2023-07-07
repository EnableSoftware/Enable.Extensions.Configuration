using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Enable.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        private const string BranchFileName = "dev-branch.txt";
        private const string DatabaseServerFileName = "dev-database-server.txt";

        private static readonly Lazy<string> Branch = new Lazy<string>(InitBranch);
        private static readonly Lazy<string> BranchPath = new Lazy<string>(InitBranchPath);
        private static readonly Lazy<string> DatabaseServer = new Lazy<string>(InitDatabaseServer);

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

        private static string ReplacePlaceholderValues(string settingValue, bool replaceSpaceWithUnderscore = true)
        {
            if (settingValue == null)
            {
                return null;
            }

            var branch = Branch.Value ?? string.Empty;
            var branchPath = BranchPath.Value ?? string.Empty;
            var databaseServer = DatabaseServer.Value ?? string.Empty;
            var machineName = Environment.MachineName ?? string.Empty;

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

            return settingValue;
        }

        private static string InitBranch()
        {
            var branchName = ReadDevelopmentEnvironmentValue(BranchFileName);

            if (branchName != null)
            {
                return branchName;
            }

            ThrowBranchFileNotFound();

            return null;
        }

        private static string InitBranchPath()
        {
            var branchFileInfo = GetDevelopmentEnvironmentFileInfo(BranchFileName);

            if (branchFileInfo != null)
            {
                return branchFileInfo.DirectoryName;
            }

            ThrowBranchFileNotFound();

            return null;
        }

        private static string InitDatabaseServer()
        {
            var databaseServer = ReadDevelopmentEnvironmentValue(DatabaseServerFileName);

            if (databaseServer != null)
            {
                return databaseServer;
            }

            throw new FileNotFoundException(
                $"DEVELOPMENT: A {DatabaseServerFileName} file was not found in your app's file path. Please create a {DatabaseServerFileName} file containing a database server name or IP address (e.g. \"(local)\") in your source repository directory.",
                BranchFileName);
        }

        private static string ReadDevelopmentEnvironmentValue(string fileName)
        {
            var devFileInfo = GetDevelopmentEnvironmentFileInfo(fileName);

            if (devFileInfo != null)
            {
                return File.ReadAllText(devFileInfo.FullName);
            }

            return null;
        }

        private static FileInfo GetDevelopmentEnvironmentFileInfo(string fileName)
        {
            var assemblyLocationUri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var assemblyLocation = assemblyLocationUri.LocalPath;
            var directoryPath = Path.GetDirectoryName(assemblyLocation);
            var directory = new DirectoryInfo(directoryPath);

            do
            {
                var filePath = Path.Combine(directory.FullName, fileName);

                if (File.Exists(filePath))
                {
                    return new FileInfo(filePath);
                }

                directory = directory.Parent;
            }
            while (directory != null);

            return null;
        }

        private static void ThrowBranchFileNotFound()
        {
            throw new FileNotFoundException(
                $"DEVELOPMENT: A {BranchFileName} file was not found in your apps's file path. Please create a {BranchFileName} file containing the development branch name (e.g. \"RC2.2\") in your source repository directory.",
                BranchFileName);
        }
    }
}
