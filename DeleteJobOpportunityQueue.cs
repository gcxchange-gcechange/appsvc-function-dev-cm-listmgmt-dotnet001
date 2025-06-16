using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class DeleteJobOpportunityQueue
    {
        private readonly ILogger<DeleteJobOpportunityQueue> _logger;

        public DeleteJobOpportunityQueue(ILogger<DeleteJobOpportunityQueue> logger)
        {
            _logger = logger;
        }

        [Function(nameof(DeleteJobOpportunityQueue))]
        public async Task RunAsync([QueueTrigger("delete", Connection = "AzureWebJobsStorage")] QueueMessage message)
        {
            _logger.LogInformation("DeleteJobOpportunityQueue received a request.");

            try
            {
                var queueMessage = JsonConvert.DeserializeObject<CMQueueMessage>(message.Body.ToString());

                if (queueMessage != null)
                {
                    var itemIds = queueMessage.Ids.Split(',').ToList();

                    if (itemIds.Any())
                    {
                        var config = new Config();
                        var client = Common.GetClient(_logger);

                        foreach (var id in itemIds)
                        {
                            try
                            {
                                var item = await client.Sites[config.SiteId].Lists[config.ListId].Items[id.Trim()].GetAsync();

                                if (item != null)
                                {
                                    await client.Sites[config.SiteId].Lists[config.ListId].Items[id.Trim()].DeleteAsync();
                                    _logger.LogInformation($"Deleted job opportunity with ID {id.Trim()}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"JobOpportunityId: {id} - {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError($"Couldn't read queue message {message}");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}");
            }

            _logger.LogInformation("DeleteJobOpportunityQueue finished processing a request.");
        }
    }
}
