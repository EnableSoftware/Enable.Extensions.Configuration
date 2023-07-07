namespace Enable.Extensions.Configuration
{
    internal class ServiceBusConnectionStringSubstitution
    {
        private readonly string _machineName;

        public ServiceBusConnectionStringSubstitution(string machineName)
        {
            _machineName = machineName;
        }

        public string Substitute(string configurationValue)
        {
            throw new NotImplementedException();
        }
    }
}
