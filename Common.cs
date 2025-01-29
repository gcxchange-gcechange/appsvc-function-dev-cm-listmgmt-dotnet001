using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using static appsvc_function_dev_cm_listmgmt_dotnet001.Auth;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public class Common
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
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
                JobOpportunity opportunity = JsonConvert.DeserializeObject<JobOpportunity>(requestBody);

                int durationInDays = opportunity.DurationId == config["durationMonthId"] ? opportunity.DurationQuantity * 30 : opportunity.DurationQuantity * 360;

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"ContactObjectId", opportunity.ContactObjectId},
                            {"ContactName", opportunity.ContactName},
                            {"DepartmentLookupId", opportunity.DepartmentId},
                            {"ContactEmail", opportunity.ContactEmail},
                            {"JobTitleEn", opportunity.JobTitleEn},
                            {"JobTitleFr", opportunity.JobTitleFr},
                            {config["jobTypeHiddenColName"], string.Join(";", opportunity.JobType.Select(jobType => jobType.ToString()))},
                            {config["programAreaHiddenColName"],  opportunity.ProgramArea.ToString()},
                            {"ClassificationCodeLookupId", opportunity.ClassificationCodeId},
                            {"ClassificationLevelLookupId", opportunity.ClassificationLevelId},
                            {"NumberOfOpportunities", opportunity.NumberOfOpportunities},
                            {"DurationLookupId", opportunity.DurationId},
                            {"ApplicationDeadlineDate", opportunity.ApplicationDeadlineDate},
                            {"JobDescriptionEn", opportunity.JobDescriptionEn},
                            {"JobDescriptionFr", opportunity.JobDescriptionFr},
                            {"WorkScheduleLookupId", opportunity.WorkScheduleId},
                            {"SecurityClearanceLookupId", opportunity.SecurityClearanceId},
                            {"LanguageComprehension", opportunity.LanguageComprehension},
                            {"LanguageRequirementLookupId", opportunity.LanguageRequirementId},
                            {"WorkArrangementLookupId", opportunity.WorkArrangementId},
                            {"ApprovedStaffing", opportunity.ApprovedStaffing},
                            {"SkillsLookupId@odata.type", "Collection(Edm.String)"},
                            {"SkillsLookupId", opportunity.SkillIds},
                            {"CityLookupId", opportunity.CityId},
                            {"DurationQuantity", opportunity.DurationQuantity},
                            {"DurationInDays", durationInDays}
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

    internal class JobOpportunity
    {
        public string ContactObjectId;
        public string ContactName;
        public string DepartmentId;
        public string ContactEmail;
        public string JobTitleEn;
        public string JobTitleFr;
        public Term[] JobType;
        public Term ProgramArea;
        public string ClassificationCodeId;
        public string ClassificationLevelId;
        public int NumberOfOpportunities;
        public string DurationId;
        public DateTime? ApplicationDeadlineDate;
        public string JobDescriptionEn;
        public string JobDescriptionFr;
        public string WorkScheduleId;
        public string CityId;
        public string SecurityClearanceId;
        public string LanguageComprehension;
        public string LanguageRequirementId;
        public string WorkArrangementId;
        public bool? ApprovedStaffing;
        public string[] SkillIds;
        public int DurationQuantity;
    }

    internal class Term
    {
        public string Label;
        public string Guid;

        public override string ToString()
        {
            return $"-1;{Label}|{Guid}";
        }
    }
}