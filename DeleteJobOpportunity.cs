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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("DeleteJobOpportunity received a request.");

            try
            {
                Config config = new Config();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string itemId = data?.ItemId;
                List<string> itemIds = itemId.Split(',').ToList();

                GraphServiceClient client = Common.GetClient(_logger);

                foreach (var id in itemIds)
                {
                    try
                    {
                        await client.Sites[config.SiteId].Lists[config.ListId].Items[id.Trim()].DeleteAsync();
                        _logger.LogInformation($"Deleted list item with ID: {id.Trim()}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"Failed to delete list item with ID: {id.Trim()}");
                    }
                }
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
