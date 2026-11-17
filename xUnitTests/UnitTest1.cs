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

            Dictionary<string, string> violations;

            try
            {
                violations = CountSeekerViolations(_jobOpportunity, _logger);
            }
            catch
            {
                Assert.True(false);
                return;
            }

            Assert.True(violations.Count == 0);
        }

        [Fact]
        public void JobShouldFailSeekerCheck()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am seeking a position in communications where I can apply my design experience. I previously worked at a federal department and I bring strong visual storytelling skills to the table.";
            _jobOpportunity.JobDescriptionFr = "Je recherche un emploi en communications numériques. Je suis parfaitement qualifié pour ce type de poste et j’ai travaillé par le passé chez un ministère fédéral.";

            Dictionary<string, string> violations;

            try
            {
                violations = CountSeekerViolations(_jobOpportunity, _logger);
            }
            catch
            {
                Assert.True(false);
                return;
            }

            Assert.True(violations.Count >= SeekerHelpers.VIOLATIONS_MAX);
        }


        [Fact]
        public void JobShouldFailWhenSeekingPosition()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am seeking a position in software development.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Single(violations);
        }

        [Fact]
        public void JobShouldFailWhenLookingForOpportunities()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am looking for a new opportunity within the organization.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenUsingFirstPersonPhrases()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I offer extensive experience in software development.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenSeekingDeployment()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am seeking deployment opportunities within the department.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenUsingPersonalIntroduction()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "My name is John and I have extensive experience in project management.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }


        [Fact]
        public void JobShouldFailWhenSeekingEmploymentInFrench()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionFr = "Je suis à la recherche d'un emploi dans le domaine informatique.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenSeekingDeploymentInFrench()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionFr = "Je cherche une affectation dans une autre direction.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenUsingPersonalIntroductionInFrench()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionFr = "Je me nomme Jean et je possède plusieurs années d'expérience.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenUsingFrenchCareerSearchPhrases()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionFr = "À la recherche de possibilités d'affectation dans la fonction publique.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }


        [Fact]
        public void JobShouldPassWhenSeekingPositionIsNotMentioned()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "We are looking for an experienced software developer to join our team.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        [Fact]
        public void JobShouldPassWhenFirstPersonPhraseIsNotAtSentenceStart()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "The successful candidate should demonstrate that they have a strong understanding of the subject.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        [Fact]
        public void JobShouldPassWhenShortPhraseIsPartOfAnotherWord()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "The classification of this position is AS-03.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        [Fact]
        public void JobShouldPassWhenIHaveAPhraseIsNotAtSentenceStart()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "Candidates who demonstrate that I have a strong understanding of the subject will be considered.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }


        [Fact]
        public void JobShouldDetectViolationsAcrossMultipleSentences()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "This is an interesting opportunity. I am looking for a new role. We offer competitive benefits.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Single(violations);
        }

        [Fact]
        public void JobShouldDetectViolationsSeparatedByNewLines()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "This is an interesting opportunity.\nI am seeking a position in IT.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Single(violations);
        }

        [Fact]
        public void JobShouldDetectViolationsInHtmlContent()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "<p>Welcome to our team.</p><p>I am seeking a position in IT.</p>";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Single(violations);
        }

        [Fact]
        public void JobShouldDetectViolationsWithHtmlEncodedCharacters()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "<p>I&#39;m looking for a new opportunity.</p>";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldDetectViolationsWithCurlyApostrophes()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I’m looking for a new opportunity in the department.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldDetectViolationsWithAccentedFrenchCharacters()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionFr = "À la recherche d'occasions dans la fonction publique.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }


        [Fact]
        public void JobShouldFailWhenEnglishTitleContainsViolation()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobTitleEn = "Seeking a Position in Information Technology";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldFailWhenFrenchTitleContainsViolation()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobTitleFr = "Recherche d'emploi en informatique";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.NotEmpty(violations);
        }

        [Fact]
        public void JobShouldPassWhenOnlyTitlesContainLegitimateContent()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobTitleEn = "Senior Software Developer";
            _jobOpportunity.JobTitleFr = "Développeur logiciel principal";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }


        [Fact]
        public void JobShouldCountMultipleViolations()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am looking for a new opportunity. I offer extensive experience in software development. My name is John.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Equal(3, violations.Count);
        }

        [Fact]
        public void JobShouldOnlyCountOneViolationPerSentence()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "I am looking for a new opportunity and I offer extensive experience.";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Single(violations);
        }

        [Fact]
        public void JobShouldNotBlockMatchesFromHTML()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "We are looking for a candidate that can <i>excel at</i>";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        [Fact]
        public void JobShouldNotBlockHTMLSeekerMatches()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobDescriptionEn = "<i am seeking>Hello, welcome to my world.</i am seeking>";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }


        [Fact]
        public void JobShouldPassWhenAllFieldsAreEmpty()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobTitleEn = "";
            _jobOpportunity.JobTitleFr = "";
            _jobOpportunity.JobDescriptionEn = "";
            _jobOpportunity.JobDescriptionFr = "";

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        [Fact]
        public void JobShouldPassWhenSomeFieldsAreNull()
        {
            ResetJobOpportunity();

            _jobOpportunity.JobTitleEn = null;
            _jobOpportunity.JobTitleFr = null;
            _jobOpportunity.JobDescriptionEn = "Join our dynamic team!";
            _jobOpportunity.JobDescriptionFr = null;

            var violations = CountSeekerViolations(_jobOpportunity, _logger);

            Assert.Empty(violations);
        }

        private void ResetJobOpportunity()
        {
            _jobOpportunity.ContactObjectId = "1";
            _jobOpportunity.ContactName = "Shea Dougherty-Gill";
            _jobOpportunity.DepartmentId = "1";
            _jobOpportunity.ContactEmail = "Shea.Dougherty-Gill@fakeemail.com";
            _jobOpportunity.ApplyEmail = "Shea.Apply@fakeemail.com";
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