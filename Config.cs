using Microsoft.Extensions.Configuration;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    internal class Config
    {
        private IConfiguration _Config;

        public string ListId { get { return _Config["listId"]; } }
        public string SiteId { get { return _Config["siteId"]; } }

        public Config()
        {
            _Config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
        }
    }

}
