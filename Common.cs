﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
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

                ValidateJobOpportunity(opportunity);

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
                            {"ApplicationDeadlineDate", opportunity.ApplicationDeadlineDate.Value.ToUniversalTime()},
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

        private static void ValidateJobOpportunity(JobOpportunity opportunity)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();

            ValidateTextField(opportunity.ContactObjectId, "ContactObjectId");
            ValidateTextField(opportunity.ContactName, "ContactName");
            ValidateLookupId(opportunity.DepartmentId, "DepartmentId");
            ValidateTextField(opportunity.ContactEmail, "ContactEmail");
            ValidateTextField(opportunity.JobTitleEn, "JobTitleEn");
            ValidateTextField(opportunity.JobTitleFr, "JobTitleFr");
            ValidateTerms(opportunity.JobType, "JobType");
            ValidateTerm(opportunity.ProgramArea, "ProgramArea");
            ValidateLookupId(opportunity.ClassificationCodeId, "ClassificationCodeId");
            ValidateLookupId(opportunity.ClassificationLevelId, "ClassificationLevelId");
            ValidateNumber(opportunity.NumberOfOpportunities, "NumberOfOpportunities");
            ValidateDateTime(opportunity.ApplicationDeadlineDate, "ApplicationDeadlineDate");
            ValidateMultiTextField(opportunity.JobDescriptionEn, "JobDescriptionEn");
            ValidateMultiTextField(opportunity.JobDescriptionFr, "JobDescriptionFr");
            ValidateLookupId(opportunity.WorkScheduleId, "WorkScheduleId");
            ValidateLookupId(opportunity.SecurityClearanceId, "SecurityClearanceId");
            ValidateLookupId(opportunity.LanguageRequirementId, "LanguageRequirementId");
            ValidateLookupId(opportunity.WorkArrangementId, "WorkArrangementId");
            ValidateLookupId(opportunity.CityId, "CityId");

            // If the opportunity is a Deployment it doesn't need a DurationId or DurationQuantity
            if (!opportunity.JobType.Any(j => j.Guid == config["deploymentJobTypeId"]))
            {
                ValidateLookupId(opportunity.DurationId, "DurationId");
                ValidateNumber(opportunity.DurationQuantity, "DurationQuantity");
            }

            if (opportunity.SkillIds == null || opportunity.SkillIds.Length == 0)
                throw new ArgumentException("Field cannot be null or empty.", "SkillIds");

            for (var i = 0; i < opportunity.SkillIds.Length; i++)
            {
                ValidateLookupId(opportunity.SkillIds[i], $"SkillIds[{i}]");
            }

            if (opportunity.ApprovedStaffing == null)
                throw new ArgumentException("Field cannot be null.", "ApprovedStaffing");

            // If a job is not Bilingual it doesn't need LanguageComprehension
            if (opportunity.LanguageRequirementId == config["bilingualLanguageRequirementId"])
            {
                ValidateTextField(opportunity.LanguageComprehension, "LanguageComprehension");

                Regex langCompRegex = new Regex(@"^[A-C]{3}-[A-C]{3}$");
                if (!langCompRegex.IsMatch(opportunity.LanguageComprehension))
                    throw new ArgumentException("Field is formatted incorrectly.", "LanguageComprehension");
            }
        }

        private static void ValidateLookupId(string value, string fieldName)
        {
            if (value == null || value == "")
                throw new ArgumentException("Field cannot be null or empty.", fieldName);

            int parseId;
            if (!int.TryParse(value, out parseId))
                throw new ArgumentException("Field must be a number", fieldName);

            if (parseId < 0)
                throw new ArgumentException("Field must be a positive number", fieldName);
        }

        private static void ValidateTextField(string value, string fieldName)
        {
            if (value == null || value == "")
                throw new ArgumentException("Field cannot be null or empty.", fieldName);

            if (value.Length > 255)
                throw new ArgumentOutOfRangeException(fieldName, "Field exceeds the maximum allowed length of 255 characters.");
        }

        private static void ValidateMultiTextField(string value, string fieldName)
        {
            if (value == null || value == "")
                throw new ArgumentException("Field cannot be null or empty.", fieldName);

            if (value.Length > 10000)
                throw new ArgumentOutOfRangeException(fieldName, "Field exceeds the maximum allowed length of 10,000 characters.");
        }

        private static void ValidateTerm(Term value, string fieldName)
        {
            if (value == null)
                throw new ArgumentException("Field cannot be null.", fieldName);

            if (value.Label == "")
                throw new ArgumentException("Field cannot be empty.", $"{fieldName}.Label");

            if (value.Guid == "")
                throw new ArgumentException("Field cannot be empty.", $"{fieldName}.Guid");
        }

        private static void ValidateTerms(Term[] value, string fieldName)
        {
            if (value == null)
                throw new ArgumentException("Field cannot be null.", fieldName);

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i].Label == "")
                    throw new ArgumentException("Field cannot be empty.", $"{fieldName}[{i}].Label");

                if (value[i].Guid == "")
                    throw new ArgumentException("Field cannot be empty.", $"{fieldName}[{i}].Guid");
            }
        }

        private static void ValidateDateTime(DateTime? value, string fieldName)
        {
            if (value == null)
                throw new ArgumentException("Field cannot be null.", fieldName);

            if (value.Value.ToUniversalTime() > DateTime.UtcNow.AddDays(1))
                throw new ArgumentException("Field must be at least 1 day into the future.", fieldName);
        }

        private static void ValidateNumber(int value, string fieldName)
        {
            if (value < 0)
                throw new ArgumentException("Field must be a positive number", fieldName);
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