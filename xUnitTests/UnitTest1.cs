using appsvc_function_dev_cm_listmgmt_dotnet001;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        public void ShouldReturnListItem()
        {
            // arrange
            string json;
            using (StreamReader r = new StreamReader(@"C:\Users\OPOSTLET\source\repos\appsvc-function-dev-cm-listmgmt-dotnet001\xUnitTests\JobOpportunity.json"))
            {
                json = r.ReadToEnd();
            }

            Console.WriteLine(json);


            // act0
            var listItem = BuildListItem(json, _logger);

            // assert
            Assert.True(listItem != null);
        }
    }
}