using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using static appsvc_function_dev_cm_listmgmt_dotnet001.Auth;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    internal class JobOpportunity
    {
        public string ContactObjectId;
        public string ContactName;
        public string Department;
        public string ContactEmail;
        public string JobTitleEn;
        public string JobTitleFr;
        public string JobType;
        public string ProgramArea;
        public string ClassificationCode;
        public string ClassificationLevel;
        public string NumberOfOpportunities;
        public string Duration;
        public DateTime? ApplicationDeadlineDate;
        public string JobDescriptionEn;
        public string JobDescriptionFr;
        public string EssentialSkills;
        public string WorkSchedule;
        public string Location;
        public string SecurityClearance;
        public string LanguageRequirement;
        public string WorkArrangement;
        public bool? ApprovedStaffing;
        public string AssetSkills;
    }

    internal class Common
    {
        public static GraphServiceClient GetClient(ILogger logger)
        {
            ROPCConfidentialTokenCredential auth = new ROPCConfidentialTokenCredential(logger);
            return new GraphServiceClient(auth);
        }

        public static ListItem BuildListItem(string requestBody, ILogger logger)
        {
            try
            {
                JobOpportunity opportunity = JsonConvert.DeserializeObject<JobOpportunity>(requestBody);
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(opportunity));

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = dictionary
                    }
                };

                logger.LogWarning($"listItem.Fields.AdditionalData.Count = {listItem.Fields.AdditionalData.Count}");
                foreach (var field in listItem.Fields.AdditionalData)
                {
                    logger.LogWarning($"{field.Key} = {field.Value}");
                    logger.LogWarning($"IsNull? {field.Value == null}");
                }

                return listItem;
            }
            catch (Exception e)
            {
                logger.LogError("Error adding a list item!!");
                logger.LogError(e.Message);
                if (e.InnerException is not null) logger.LogError(e.InnerException.Message);
                logger.LogError(e.StackTrace);

                return null;
            }
        }
    }

}
