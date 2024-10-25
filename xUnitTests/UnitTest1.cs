namespace xUnitTests
{
    public class UnitTest1
    {
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
    }
}