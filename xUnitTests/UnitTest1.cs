using appsvc_function_dev_cm_listmgmt_dotnet001;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Emit;
using static appsvc_function_dev_cm_listmgmt_dotnet001.Common;
using static appsvc_function_dev_cm_listmgmt_dotnet001.SeekerHelpers;

namespace xUnitTests
{
    public class UnitTest1
    {
        private ILogger _logger;
        private JobOpportunity _jobOpportunity = new JobOpportunity();

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
        public void JobShouldPassValidation()
        {
            ResetJobOpportunity();

            try
            {
                ValidateJobOpportunity(_jobOpportunity);
            }
            catch
            {
                Assert.True(false);
                return;
            }

            Assert.True(true);
        }

        [Fact]
        public void JobShouldFailValidation_DeadlineNotUTC()
        {
            ResetJobOpportunity();
            _jobOpportunity.ApplicationDeadlineDate = DateTime.Now;

            try
            {
                ValidateJobOpportunity(_jobOpportunity);
            }
            catch
            {
                Assert.True(true);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public void JobShouldFailValidation_DeadlineNotMinimum()
        {
            ResetJobOpportunity();
            _jobOpportunity.ApplicationDeadlineDate = DateTime.UtcNow;

            try
            {
                ValidateJobOpportunity(_jobOpportunity);
            }
            catch
            {
                Assert.True(true);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public void JobShouldPassSeekerCheck()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "Are you passionate about visual storytelling and creating designs that inspire action? Join the Digital Innovation and Change Enablement (DSG, DTP) team as a Communications Designer!\n\nIn this hybrid role at Carling Campus, you will work closely with a dynamic team to develop visuals that support internal communications and digital change initiatives. Strong graphic design skills are essential, as well as a solid understanding of communications and marketing principles. Additional experience in photography or videography, change management, or a passion for digital adoption will be considered an asset. Above all, we are looking for someone who loves visual storytelling and thrives in a collaborative environment.\n\nThis position requires Bilingual BBB / Reliability Clearance. \n\nIf you are at AS-03 level and ready to make an impact, we would like to hear from you at catherine.laberge3@forces.gc.ca.";
            _jobOpportunity.JobDescriptionFr = "Êtes-vous passionné(e) par la narration visuelle et la création de designs qui inspirent à passer à l’action? Joignez-vous à l’équipe Innovation numérique et facilitation du changement (GSN, PTN) en tant que Concepteur(trice) en communications!\n\nDans ce rôle hybride au Campus Carling, vous travaillerez en étroite collaboration avec une équipe dynamique afin de développer des visuels qui soutiennent les communications internes et les initiatives de changement numérique. De solides compétences en conception graphique sont essentielles, ainsi qu’une bonne compréhension des principes de communication et de marketing. Une expérience supplémentaire en photographie ou vidéographie, en gestion du changement, ou une passion pour l’adoption numérique sera considérée comme un atout. Surtout, nous recherchons quelqu’un qui aime la narration visuelle et qui s’épanouit dans un environnement collaboratif.\n\nCe poste exige le bilinguisme BBB et une cote de fiabilité.\n\nSi vous êtes au niveau AS-03 et prêt(e) à avoir un impact, envoyez votre CV à catherine.laberge3@forces.gc.ca.";

            int violations = 0;

            try
            {
                violations = CountSeekerViolations(_jobOpportunity, _logger);
            }
            catch
            {
                Assert.True(false);
                return;
            }

            Assert.Equal(0, violations);
        }

        [Fact]
        public void JobShouldFailSeekerCheck()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am seeking a position in communications where I can apply my design experience. I previously worked at a federal department and I bring strong visual storytelling skills to the table.";
            _jobOpportunity.JobDescriptionFr = "Je recherche un emploi en communications numériques. Je suis parfaitement qualifié pour ce type de poste et j’ai travaillé par le passé chez un ministère fédéral.";

            int violations = 0;

            try
            {
                violations = CountSeekerViolations(_jobOpportunity, _logger);
            }
            catch
            {
                Assert.True(false);
                return;
            }

            Assert.True(violations >= SeekerHelpers.VIOLATIONS_MAX);
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

        private void ResetJobOpportunity()
        {
            _jobOpportunity.ContactObjectId = "1";
            _jobOpportunity.ContactName = "Shea Dougherty-Gill";
            _jobOpportunity.DepartmentId = "1";
            _jobOpportunity.ContactEmail = "Shea.Dougherty-Gill@fakeemail.com";
            _jobOpportunity.JobTitleEn = "Job TItle EN";
            _jobOpportunity.JobTitleFr = "Job TItle FR";
            _jobOpportunity.JobType = [new Term { Label = "Test Job Type", Guid = "7eb00e55-4683-4b49-b83a-1b52090adb02" }];
            _jobOpportunity.ProgramArea = new Term { Label = "Test Program Area", Guid = "971289fe-1c66-42c9-a87b-53170c9116e1" };
            _jobOpportunity.ClassificationCodeId = "1";
            _jobOpportunity.ClassificationLevelId = "1";
            _jobOpportunity.NumberOfOpportunities = 1;
            _jobOpportunity.DurationId = "1";
            _jobOpportunity.ApplicationDeadlineDate = DateTime.Now.AddDays(30).ToUniversalTime();
            _jobOpportunity.JobDescriptionEn = "This is the job description EN";
            _jobOpportunity.JobDescriptionFr = "This is the job description FR";
            _jobOpportunity.WorkScheduleId = "1";
            _jobOpportunity.CityId = "1";
            _jobOpportunity.SecurityClearanceId = "1";
            _jobOpportunity.LanguageComprehension = "ABC-ABC";
            _jobOpportunity.LanguageRequirementId = "1";
            _jobOpportunity.WorkArrangementId = "1";
            _jobOpportunity.ApprovedStaffing = true;
            _jobOpportunity.SkillIds = ["1", "2", "3"];
            _jobOpportunity.DurationQuantity = 3;
        }
    }
}