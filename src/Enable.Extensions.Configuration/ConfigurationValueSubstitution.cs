namespace Enable.Extensions.Configuration;

internal class ConfigurationValueSubstitution
{
    private readonly Lazy<string> _branch;
    private readonly Lazy<string> _branchPath;
    private readonly Lazy<string> _databaseServer;
    private readonly string _machineName;
    private readonly string _serviceBusSharedAccessKey;

    public ConfigurationValueSubstitution(
        Lazy<string> branch,
        Lazy<string> branchPath,
        Lazy<string> databaseServer,
        string machineName,
        string serviceBusSharedAccessKey)
    {
        _branch = branch;
        _branchPath = branchPath;
        _databaseServer = databaseServer;
        _machineName = machineName;
        _serviceBusSharedAccessKey = serviceBusSharedAccessKey;
    }

    public string Substitute(string settingValue)
    {
        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return settingValue;
        }

        return settingValue
            .ReplaceIfPresent("{Branch}", _branch)
            .ReplaceIfPresent("{BranchPath}", _branchPath)
            .ReplaceIfPresent("{DatabaseServer}", _databaseServer)
            .Replace("{MachineName}", _machineName)
            .Replace("{ServiceBusSharedAccessKey}", _serviceBusSharedAccessKey);
    }
}
