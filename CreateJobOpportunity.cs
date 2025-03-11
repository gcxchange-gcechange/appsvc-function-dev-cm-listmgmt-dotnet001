using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class CreateJobOpportunity
    {
        private readonly ILogger<CreateJobOpportunity> _logger;

        public CreateJobOpportunity(ILogger<CreateJobOpportunity> logger)
        {
            _logger = logger;
        }

        [Function("CreateJobOpportunity")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("CreateJobOpportunity received a request.");

            bool _exception = false;

            try
            {
                Config config = new Config();
                var listItem = Common.BuildListItem(new StreamReader(req.Body).ReadToEnd(), _logger);
                GraphServiceClient client = Common.GetClient(_logger);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(listItem.Fields.AdditionalData);

                await client.Sites[config.SiteId].Lists[config.ListId].Items.PostAsync(listItem);
            }
            catch (Exception e)
            {
                _exception = true;
                _logger.LogError(e.Message);
                if (e.InnerException is not null) _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.StackTrace);
            }

            _logger.LogInformation("CreateJobOpportunity processed a request.");

            if (!_exception)
            {
                return new OkResult();
            } else
            {
                return new BadRequestResult();
            }
        }
    }
}
