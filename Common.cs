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

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"ContactObjectIdTest", opportunity.ContactObjectId},
                            {"ContactNameTest", opportunity.ContactName},
                            {config["departmentHiddenColName"], opportunity.Department.ToString()},
                            {"ContactEmailTest", opportunity.ContactEmail},
                            {"JobTitleEnTest", opportunity.JobTitleEn},
                            {"JobTitleFrTest", opportunity.JobTitleFr},
                            {config["jobTypeHiddenColName"], string.Join(";", opportunity.JobType.Select(jobType => jobType.ToString()))},
                            {config["programAreaHiddenColName"],  opportunity.ProgramArea.ToString()},
                            {config["classificationCodeHiddenColName"], opportunity.ClassificationCode.ToString()},
                            {"ClassificationLevelTestLookupId", opportunity.ClassificationLevelLookupId},
                            {"NumberOfOpportunitiesTest", opportunity.NumberOfOpportunities},
                            {config["durationHiddenColName"], opportunity.Duration.ToString()},
                            {"ApplicationDeadlineDateTest", opportunity.ApplicationDeadlineDate},
                            {"JobDescriptionEnTest", opportunity.JobDescriptionEn},
                            {"JobDescriptionFrTest", opportunity.JobDescriptionFr},
                            {config["workScheduleHiddenColName"], opportunity.WorkSchedule.ToString()},
                            {config["locationHiddenColName"], opportunity.Location.ToString()},
                            {config["securityClearanceHiddenColName"], opportunity.SecurityClearance.ToString()},
                            {config["languageRequirementHiddenColName"], opportunity.LanguageRequirement.ToString()},
                            {config["workArrangementHiddenColName"], opportunity.WorkArrangement.ToString()},
                            {"ApprovedStaffingTest", opportunity.ApprovedStaffing},
                            {"SkillsTestLookupId@odata.type", "Collection(Edm.String)"},
                            {"SkillsTestLookupId", opportunity.Skills}
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
        public Term Department;
        public string ContactEmail;
        public string JobTitleEn;
        public string JobTitleFr;
        public Term[] JobType;
        public Term ProgramArea;
        public Term ClassificationCode;
        public string ClassificationLevelLookupId;
        public string NumberOfOpportunities;
        public Term Duration;
        public DateTime? ApplicationDeadlineDate;
        public string JobDescriptionEn;
        public string JobDescriptionFr;
        public Term WorkSchedule;
        public Term Location;
        public Term SecurityClearance;
        public Term LanguageRequirement;
        public Term WorkArrangement;
        public bool? ApprovedStaffing;
        public string[] Skills;
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