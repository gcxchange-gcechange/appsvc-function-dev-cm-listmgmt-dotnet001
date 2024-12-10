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
                JobOpportunity opportunity = JsonConvert.DeserializeObject<JobOpportunity>(requestBody);

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"ContactObjectIdTest", opportunity.ContactObjectId},
                            {"ContactNameTest", opportunity.ContactName},
                            {"ha560ef4634b48b49bcb4e0358a668ed", opportunity.Department.ToString()},                                                            //DepartmentTest
                            {"ContactEmailTest", opportunity.ContactEmail},
                            {"JobTitleEnTest", opportunity.JobTitleEn},
                            {"JobTitleFrTest", opportunity.JobTitleFr},
                            {"n5a8092d214642c391695b072c2b6ebf", string.Join(";", opportunity.JobType.Select(jobType => jobType.ToString()))},                  // JobTypeTest
                            {"h9ed8b922e6b4ec68698b62ca9658243",  opportunity.ProgramArea.ToString()},                                                          // ProgramAreaTest
                            {"laf5fd57fe9641c1a283d71d2fb42bfa", opportunity.ClassificationCode.ToString()},                                                    // ClassificationCodeTest
                            {"ClassificationLevelTestLookupId", opportunity.ClassificationLevelLookupId},
                            {"NumberOfOpportunitiesTest", opportunity.NumberOfOpportunities},
                            {"cda78f5757a94444aec25da23261bb64", opportunity.Duration.ToString()},                                                              // DurationTest
                            {"ApplicationDeadlineDateTest", opportunity.ApplicationDeadlineDate},
                            {"JobDescriptionEnTest", opportunity.JobDescriptionEn},
                            {"JobDescriptionFrTest", opportunity.JobDescriptionFr},
                            {"EssentialSkillsTest", opportunity.EssentialSkills},
                            {"ka65c2b89e214ab6a1b1a5c97dc0c30e", opportunity.WorkSchedule.ToString()},                                                          // WorkScheduleTest
                            {"i88ce72e4a594b0cb30693e3e0b90194", opportunity.Location.ToString()},                                                              // LocationTest
                            {"b78aa598489d482aa3274ce9f1e83da2", opportunity.SecurityClearance.ToString()},                                                     // SecurityClearanceTest
                            {"e54ee833fac249acaafc47a309855bde", opportunity.LanguageRequirement.ToString()},                                                   // LanguageRequirementTest
                            {"pfe8022cbeee4db4826595bc01c1601c", opportunity.WorkArrangement.ToString()},                                                       // WorkArrangementTest
                            {"ApprovedStaffingTest", opportunity.ApprovedStaffing},
                            {"AssetSkillsTest", opportunity.AssetSkills}
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
        public string EssentialSkills;
        public Term WorkSchedule;
        public Term Location;
        public Term SecurityClearance;
        public Term LanguageRequirement;
        public Term WorkArrangement;
        public bool? ApprovedStaffing;
        public string AssetSkills;
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