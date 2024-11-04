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
        public string DepartmentLookupId;
        public string ContactEmail;
        public string JobTitleEn;
        public string JobTitleFr;
        public string[] JobTypeLookupId;
        public string ProgramAreaLookupId;
        public string ClassificationCodeLookupId;
        public string ClassificationLevelLookupId;
        public string NumberOfOpportunities;
        public string DurationLookupId;
        public DateTime? ApplicationDeadlineDate;
        public string JobDescriptionEn;
        public string JobDescriptionFr;
        public string EssentialSkills; 
        public string WorkScheduleLookupId;
        public string LocationLookupId;
        public string SecurityClearanceLookupId;
        public string LanguageRequirementLookupId;
        public string WorkArrangementLookupId;
        public bool? ApprovedStaffing;
        public string AssetSkills;
        public string CityLookupId;
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

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"ContactObjectId", opportunity.ContactObjectId},
                            {"ContactName", opportunity.ContactName},
                            {"DepartmentLookupId", opportunity.DepartmentLookupId},
                            {"ContactEmail", opportunity.ContactEmail},
                            {"JobTitleEn", opportunity.JobTitleEn},
                            {"JobTitleFr", opportunity.JobTitleFr},
                            {"JobTypeLookupId@odata.type", "Collection(Edm.String)"},
                            {"JobTypeLookupId", opportunity.JobTypeLookupId},
                            {"ProgramAreaLookupId", opportunity.ProgramAreaLookupId},
                            {"ClassificationCodeLookupId", opportunity.ClassificationCodeLookupId},
                            {"ClassificationLevelLookupId", opportunity.ClassificationLevelLookupId},
                            {"NumberOfOpportunities", opportunity.NumberOfOpportunities},
                            {"DurationLookupId", opportunity.DurationLookupId},
                            {"ApplicationDeadlineDate", opportunity.ApplicationDeadlineDate},
                            {"JobDescriptionEn", opportunity.JobDescriptionEn},
                            {"JobDescriptionFr", opportunity.JobDescriptionFr},
                            {"EssentialSkills", opportunity.EssentialSkills},
                            {"WorkScheduleLookupId", opportunity.WorkScheduleLookupId},
                            {"LocationLookupId", opportunity.LocationLookupId},
                            {"SecurityClearanceLookupId", opportunity.SecurityClearanceLookupId},
                            {"LanguageRequirementLookupId", opportunity.LanguageRequirementLookupId},
                            {"WorkArrangementLookupId", opportunity.WorkArrangementLookupId},
                            {"ApprovedStaffing", opportunity.ApprovedStaffing},
                            {"AssetSkills", opportunity.AssetSkills},
                            {"CityLookupId", opportunity.CityLookupId}
                        }
                    }
                };

                //logger.LogWarning($"listItem.Fields.AdditionalData.Count = {listItem.Fields.AdditionalData.Count}");
                //foreach (var field in listItem.Fields.AdditionalData)
                //{
                //    logger.LogWarning($"{field.Key} = {field.Value}");
                //    logger.LogWarning($"IsNull? {field.Value == null}");
                //}

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
