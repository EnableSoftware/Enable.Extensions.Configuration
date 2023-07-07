namespace Enable.Extensions.Configuration;

internal class ConfigurationValueSubstitution
{
    private readonly string _branch;
    private readonly string _branchPath;
    private readonly string _databaseServer;
    private readonly string _machineName;
    private readonly string _serviceBusSharedAccessKey;

    public ConfigurationValueSubstitution(
        string branch,
        string branchPath,
        string databaseServer,
        string machineName,
        string serviceBusSharedAccessKey)
    {
        _branch = branch.Replace(' ', '_');
        _branchPath = branchPath.Replace(' ', '_');
        _databaseServer = databaseServer.Replace(' ', '_');
        _machineName = machineName.Replace(' ', '_');
        _serviceBusSharedAccessKey = serviceBusSharedAccessKey;
    }

    public string? Substitute(string settingValue)
    {
        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return settingValue;
        }

        return settingValue
            .Replace("{Branch}", _branch)
            .Replace("{BranchPath}", _branchPath)
            .Replace("{DatabaseServer}", _databaseServer)
            .Replace("{MachineName}", _machineName)
            .Replace("{ServiceBusSharedAccessKey}", _serviceBusSharedAccessKey);
    }
}
