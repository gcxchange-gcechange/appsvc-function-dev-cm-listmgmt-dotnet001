using FuzzySharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static appsvc_function_dev_cm_listmgmt_dotnet001.Auth;
using static appsvc_function_dev_cm_listmgmt_dotnet001.SeekerHelpers;

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

                // Loads dynamic property aliases for JobOpportunity
                PropertyAliasMapper.LoadAliases(config);

                JobOpportunity opportunity = JsonConvert.DeserializeObject<JobOpportunity>(requestBody);

                ValidateJobOpportunity(opportunity);

                var violations = CountSeekerViolations(opportunity, logger);
                if (violations >= SeekerHelpers.VIOLATIONS_MAX)
                {
                    logger.LogError($"Post blocked due to job seeking.");
                    throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "Job seeking is prohibited.", new { Violations = violations });
                }

                // data cleanup: if DurationId is empty then give it a value of 0
                if (opportunity.DurationId == string.Empty)
                    opportunity.DurationId = "0";

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ContactObjectId)), opportunity.ContactObjectId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ContactName)), opportunity.ContactName },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.DepartmentId)), opportunity.DepartmentId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ContactEmail)), opportunity.ContactEmail },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.JobTitleEn)), opportunity.JobTitleEn },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.JobTitleFr)), opportunity.JobTitleFr },
                            { config["jobTypeHiddenColName"], string.Join(";", opportunity.JobType.Select(jobType => jobType.ToString()))},
                            { config["programAreaHiddenColName"], opportunity.ProgramArea.ToString()},
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ClassificationCodeId)), opportunity.ClassificationCodeId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ClassificationLevelId)), opportunity.ClassificationLevelId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.NumberOfOpportunities)), opportunity.NumberOfOpportunities },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.DurationId)), opportunity.DurationId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ApplicationDeadlineDate)), opportunity.ApplicationDeadlineDate },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.JobDescriptionEn)), opportunity.JobDescriptionEn },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.JobDescriptionFr)), opportunity.JobDescriptionFr },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.WorkScheduleId)), opportunity.WorkScheduleId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.SecurityClearanceId)), opportunity.SecurityClearanceId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.LanguageComprehension)), opportunity.LanguageComprehension },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.LanguageRequirementId)), opportunity.LanguageRequirementId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.WorkArrangementId)), opportunity.WorkArrangementId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.ApprovedStaffing)), opportunity.ApprovedStaffing },
                            { $"{PropertyAliasMapper.GetAlias(nameof(opportunity.SkillIds))}@odata.type", "Collection(Edm.String)"},
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.SkillIds)), opportunity.SkillIds },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.CityId)), opportunity.CityId },
                            { PropertyAliasMapper.GetAlias(nameof(opportunity.DurationQuantity)), opportunity.DurationQuantity },
                            { config["durationInDays_Alias"], CalculateDurationInDays(opportunity, config)}
                        }
                    }
                };

                return listItem;
            }
            catch (HttpResponseException e)
            {
                throw;
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

        private static int CalculateDurationInDays(JobOpportunity opportunity, IConfigurationRoot config)
        {
            var durationId = int.Parse(opportunity.DurationId);

            if (durationId == 0)
            {
                return 0;
            }
            else if (durationId == int.Parse(config["durationYearId"]))
            {
                return 365 * opportunity.DurationQuantity;
            }
            else if (durationId == int.Parse(config["durationMonthId"]))
            {
                return (int)Math.Round(365.0 / 12.0 * opportunity.DurationQuantity, MidpointRounding.AwayFromZero);
            }
            else if (durationId == int.Parse(config["durationWeekId"]))
            {
                return (int)Math.Round(365.0 / 52.0 * opportunity.DurationQuantity, MidpointRounding.AwayFromZero);
            }

            throw new ArgumentException("Failed to map to one of the following: [durationYearId, durationMonthId, durationWeekId]", "DurationId");
        }

        public static void ValidateJobOpportunity(JobOpportunity opportunity)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();

            // Loads dynamic property aliases for JobOpportunity
            PropertyAliasMapper.LoadAliases(config);

            ValidateTextField(opportunity.ContactObjectId, PropertyAliasMapper.GetAlias(nameof(opportunity.ContactObjectId)));
            ValidateTextField(opportunity.ContactName, PropertyAliasMapper.GetAlias(nameof(opportunity.ContactName)));
            ValidateLookupId(opportunity.DepartmentId, PropertyAliasMapper.GetAlias(nameof(opportunity.DepartmentId)));
            ValidateTextField(opportunity.ContactEmail, PropertyAliasMapper.GetAlias(nameof(opportunity.ContactEmail)));
            ValidateTextField(opportunity.JobTitleEn, PropertyAliasMapper.GetAlias(nameof(opportunity.JobTitleEn)));
            ValidateTextField(opportunity.JobTitleFr, PropertyAliasMapper.GetAlias(nameof(opportunity.JobTitleFr)));
            ValidateTerms(opportunity.JobType, PropertyAliasMapper.GetAlias(nameof(opportunity.JobType)));
            ValidateTerm(opportunity.ProgramArea, PropertyAliasMapper.GetAlias(nameof(opportunity.ProgramArea)));
            ValidateLookupId(opportunity.ClassificationCodeId, PropertyAliasMapper.GetAlias(nameof(opportunity.ClassificationCodeId)));
            ValidateLookupId(opportunity.ClassificationLevelId, PropertyAliasMapper.GetAlias(nameof(opportunity.ClassificationLevelId)));
            ValidateNumber(opportunity.NumberOfOpportunities, PropertyAliasMapper.GetAlias(nameof(opportunity.NumberOfOpportunities)));
            ValidateDateTime(opportunity.ApplicationDeadlineDate, PropertyAliasMapper.GetAlias(nameof(opportunity.ApplicationDeadlineDate)));
            ValidateMultiTextField(opportunity.JobDescriptionEn, PropertyAliasMapper.GetAlias(nameof(opportunity.JobDescriptionEn)));
            ValidateMultiTextField(opportunity.JobDescriptionFr, PropertyAliasMapper.GetAlias(nameof(opportunity.JobDescriptionFr)));
            ValidateLookupId(opportunity.WorkScheduleId, PropertyAliasMapper.GetAlias(nameof(opportunity.WorkScheduleId)));
            ValidateLookupId(opportunity.SecurityClearanceId, PropertyAliasMapper.GetAlias(nameof(opportunity.SecurityClearanceId)));
            ValidateLookupId(opportunity.LanguageRequirementId, PropertyAliasMapper.GetAlias(nameof(opportunity.LanguageRequirementId)));
            ValidateLookupId(opportunity.WorkArrangementId, PropertyAliasMapper.GetAlias(nameof(opportunity.WorkArrangementId)));
            ValidateLookupId(opportunity.CityId, PropertyAliasMapper.GetAlias(nameof(opportunity.CityId)));

            // If the opportunity is a Deployment it doesn't need a DurationId or DurationQuantity
            if (!opportunity.JobType.Any(j => j.Guid == config["deploymentJobTypeId"]))
            {
                ValidateLookupId(opportunity.DurationId, PropertyAliasMapper.GetAlias(nameof(opportunity.DurationId)));
                ValidateNumber(opportunity.DurationQuantity, PropertyAliasMapper.GetAlias(nameof(opportunity.DurationQuantity)));
            }

            if (opportunity.SkillIds == null || opportunity.SkillIds.Length == 0)
                throw new ArgumentException("Field cannot be null or empty.", PropertyAliasMapper.GetAlias(nameof(opportunity.SkillIds)));

            for (var i = 0; i < opportunity.SkillIds.Length; i++)
            {
                ValidateLookupId(opportunity.SkillIds[i], $"{PropertyAliasMapper.GetAlias(nameof(opportunity.SkillIds))}[{i}]");
            }

            if (opportunity.ApprovedStaffing == null)
                throw new ArgumentException("Field cannot be null.", PropertyAliasMapper.GetAlias(nameof(opportunity.ApprovedStaffing)));

            // If a job is not Bilingual it doesn't need LanguageComprehension
            if (opportunity.LanguageRequirementId == config["bilingualLanguageRequirementId"])
            {
                ValidateTextField(opportunity.LanguageComprehension, PropertyAliasMapper.GetAlias(nameof(opportunity.LanguageComprehension)));

                Regex langCompRegex = new Regex(@"^[A-C,E]{3}-[A-C,E]{3}$");
                if (!langCompRegex.IsMatch(opportunity.LanguageComprehension))
                    throw new ArgumentException("Field is formatted incorrectly.", PropertyAliasMapper.GetAlias(nameof(opportunity.LanguageComprehension)));
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

            if (value.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"DateTimeKind expected Utc but was {value.Value.Kind}", fieldName);

            if (value.Value < DateTime.UtcNow.AddDays(7))
                throw new ArgumentException("Field must be at least 7 days into the future.", fieldName);
        }

        private static void ValidateNumber(int value, string fieldName)
        {
            if (value < 0)
                throw new ArgumentException("Field must be a positive number", fieldName);
        }
    }

    public class JobOpportunity
    {
        public string ContactObjectId { get; set; }
        public string ContactName { get; set; }
        public string DepartmentId { get; set; }
        public string ContactEmail { get; set; }
        public string JobTitleEn { get; set; }
        public string JobTitleFr { get; set; }
        public Term[] JobType { get; set; }
        public Term ProgramArea { get; set; }
        public string ClassificationCodeId { get; set; }
        public string ClassificationLevelId { get; set; }
        public int NumberOfOpportunities { get; set; }
        public string DurationId { get; set; }
        public DateTime? ApplicationDeadlineDate { get; set; }
        public string JobDescriptionEn { get; set; }
        public string JobDescriptionFr { get; set; }
        public string WorkScheduleId { get; set; }
        public string CityId { get; set; }
        public string SecurityClearanceId { get; set; }
        public string LanguageComprehension { get; set; }
        public string LanguageRequirementId { get; set; }
        public string WorkArrangementId { get; set; }
        public bool? ApprovedStaffing { get; set; }
        public string[] SkillIds { get; set; }
        public int DurationQuantity { get; set; }
    }

    public class Term
    {
        public string Label { get; set; }
        public string Guid { get; set; }

        public override string ToString()
        {
            return $"-1;{Label}|{Guid}";
        }
    }

    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public object? Details { get; }

        public HttpResponseException(HttpStatusCode statusCode, string message, object? details = null)
        : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }
}
