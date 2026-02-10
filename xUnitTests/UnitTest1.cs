using appsvc_function_dev_cm_listmgmt_dotnet001;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Emit;
using static appsvc_function_dev_cm_listmgmt_dotnet001.Common;

namespace xUnitTests
{
    public class UnitTest1
    {
        private ILogger _logger;

        public UnitTest1()
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = factory.CreateLogger("UnitTest");
            _logger.LogInformation("Hello World! Logging is {Description}.", "fun");
        }

        [Fact]
        public void ShouldReturnTheSum()
        {
            // the 3 "A"s - Arrange, Act, Assert

            // arrange
            var addend1 = 2;
            var addend2 = 3;

            // act - one action per test
            var sum = addend1 + addend2;

            //assert (expectation, actual)
            //can be more than one assertion in a test but opinions vary
            Assert.Equal(5, sum);
        }

        [Fact]
        public void ShouldValidateJobOpportunity()
        {
            var job = new JobOpportunity();

            job.ContactObjectId = "1";
            job.ContactName = "Shea Dougherty-Gill";
            job.DepartmentId = "1";
            job.ContactEmail = "Shea.Dougherty-Gill@fakeemail.com";
            job.JobTitleEn = "Job TItle EN";
            job.JobTitleFr = "Job TItle FR";
            job.JobType = [ new Term { Label = "Test Job Type", Guid = "7eb00e55-4683-4b49-b83a-1b52090adb02" } ];
            job.ProgramArea = new Term { Label = "Test Program Area", Guid = "971289fe-1c66-42c9-a87b-53170c9116e1" };
            job.ClassificationCodeId = "1";
            job.ClassificationLevelId = "1";
            job.NumberOfOpportunities = 1;
            job.DurationId = "1";
            job.ApplicationDeadlineDate = DateTime.Now.AddDays(30).ToUniversalTime();
            job.JobDescriptionEn = "This is the job description EN";
            job.JobDescriptionFr = "This is the job description FR";
            job.WorkScheduleId = "1";
            job.CityId = "1";
            job.SecurityClearanceId = "1";
            job.LanguageComprehension = "ABC-ABC";
            job.LanguageRequirementId = "1";
            job.WorkArrangementId = "1";
            job.ApprovedStaffing = true;
            job.SkillIds = ["1", "2", "3"];
            job.DurationQuantity = 3;

            try
            {
                ValidateJobOpportunity(job);
            }
            catch
            {
                Assert.True(false);
            }

            Assert.True(true);
        }

        //[Fact]
        //public void ShouldReturnListItem()
        //{
        //    // arrange
        //    string json;
        //    using (StreamReader r = new StreamReader(@"C:\Users\OPOSTLET\source\repos\appsvc-function-dev-cm-listmgmt-dotnet001\xUnitTests\JobOpportunity.json"))
        //    {
        //        json = r.ReadToEnd();
        //    }

        //    Console.WriteLine(json);


        //    // act0
        //    var listItem = BuildListItem(json, _logger);

        //    // assert
        //    Assert.True(listItem != null);
        //}
    }
}