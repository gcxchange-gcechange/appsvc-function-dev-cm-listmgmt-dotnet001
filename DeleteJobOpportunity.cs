using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class DeleteJobOpportunity
    {
        private readonly ILogger<DeleteJobOpportunity> _logger;

        public DeleteJobOpportunity(ILogger<DeleteJobOpportunity> logger)
        {
            _logger = logger;
        }

        [Function("DeleteJobOpportunity")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("DeleteJobOpportunity received a request.");

            try
            {
                Config config = new Config();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string itemId = data?.ItemId;

                _logger.LogInformation($"Delete list item with ID: {itemId}");

                GraphServiceClient client = Common.GetClient(_logger);
                await client.Sites[config.SiteId].Lists[config.ListId].Items[itemId].DeleteAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                if (e.InnerException is not null) _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.StackTrace);
            }

            _logger.LogInformation("DeleteJobOpportunity processed a request.");

            return new OkResult();
        }
    }
}
