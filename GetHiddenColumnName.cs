using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class GetHiddenColumnName
    {
        private readonly ILogger<GetHiddenColumnName> _logger;

        public GetHiddenColumnName(ILogger<GetHiddenColumnName> logger)
        {
            _logger = logger;
        }

        [Function("GetHiddenColumnName")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("GetHiddenColumnName received a request.-2");

            var columnNames = new List<string>();

            try
            {
                Config config = new Config();
                GraphServiceClient client = Common.GetClient(_logger);
                _logger.LogInformation("Hidden culumn call");
                var columns = await client
                    .Sites[config.SiteId]
                    .Lists[config.ListId]
                    .Columns
                    .GetAsync(requestConfiguration => 
                    {
                        requestConfiguration.QueryParameters.Select = ["id", "name", "hidden", "displayName"];
                    });

                if (columns?.Value != null)
                {
                    foreach (var column in columns.Value)
                    {
                        if (column.DisplayName == "JobType_0" || column.DisplayName == "ProgramArea_0")
                        {
                            var columnName = $"{column.DisplayName} - {column.Name}";

                            columnNames.Add(columnName);

                            _logger.LogInformation(columnName);

                            if (columnNames.Count == 2)
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                if (e.InnerException is not null) _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.StackTrace);

                return new BadRequestResult();
            }

            return new OkObjectResult(string.Join(", ", columnNames));
        }
    }
}
