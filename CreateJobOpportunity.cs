using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using static appsvc_function_dev_cm_listmgmt_dotnet001.SeekerHelpers;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("CreateJobOpportunity received a request.");

            bool _exception = false;

            string listItemId;

            try
            {
                Config config = new Config();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string itemId = data?.ItemId;
                var listItem = Common.BuildListItem(requestBody, _logger);

                GraphServiceClient client = Common.GetClient(_logger);
                string json = JsonConvert.SerializeObject(listItem.Fields.AdditionalData);

                var newListItem = await client.Sites[config.SiteId].Lists[config.ListId].Items.PostAsync(listItem);
                listItemId = newListItem.Id;
            }
            catch (SeekerViolationException ex)
            {
                return new UnprocessableEntityObjectResult(new
                {
                    Message = ex.Message,
                    Violations = ex.Violations
                });
            }
            catch (HttpResponseException e)
            {
                return new ObjectResult(e.Details)
                {
                    StatusCode = (int)e.StatusCode
                };
            }
            catch (Exception e)
            {
                _exception = true;
                _logger.LogError(e.Message);
                if (e.InnerException is not null) _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.StackTrace);
                listItemId = "";
            }

            _logger.LogInformation($"listItemId = {listItemId}");
            _logger.LogInformation("CreateJobOpportunity processed a request.");

            if (!_exception)
            {
                return new OkObjectResult(listItemId);
            } else
            {
                return new BadRequestResult();
            }
        }
    }
}