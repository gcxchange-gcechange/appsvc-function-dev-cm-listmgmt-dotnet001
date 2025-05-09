using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class UpdateJobOpportunity
    {
        private readonly ILogger<UpdateJobOpportunity> _logger;

        public UpdateJobOpportunity(ILogger<UpdateJobOpportunity> logger)
        {
            _logger = logger;
        }

        [Function("UpdateJobOpportunity")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("UpdateJobOpportunity received a request.");

            StatusCodeResult result = new OkResult();

            try
            {
                Config config = new Config();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string itemId = data?.ItemId;
                var listItem = Common.BuildListItem(requestBody, _logger);
                string ContactEmail = listItem.Fields.AdditionalData["ContactEmail"].ToString();

                if (ClaimsPrincipalParser.CanUpdate(req, ContactEmail, _logger))
                {
                    GraphServiceClient client = Common.GetClient(_logger);
                    await client.Sites[config.SiteId].Lists[config.ListId].Items[itemId].Fields.PatchAsync(listItem.Fields);
                }
                else
                {
                    _logger.LogWarning($"Unauthorized update attempted by logged in user {ClaimsPrincipalParser.GetUserEmail(req, _logger)} on JobOpportunityId {itemId}.");
                    result = new BadRequestResult();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                if (e.InnerException is not null) _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.StackTrace);
            }

            _logger.LogInformation("UpdateJobOpportunity processed a request.");

            return result;
        }
    }
}