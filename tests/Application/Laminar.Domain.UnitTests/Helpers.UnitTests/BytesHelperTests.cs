using Laminar.Domain.Helpers;

namespace Laminar.Domain.UnitTests.Helpers.UnitTests;

public class BytesHelperTests
{
    [Fact]
    public void ShouldMatchSuccess()
    {
        byte[]?[] testData =
        [
            [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
            [99, 100, 90, 84, 35, 31],
            [10, 9, 8, 7, 6, 5, 4, 3, 2, 1],
            [85, 13, 85, 12, 42, 73, 13, 74, 13, 75, 31, 58],
            null
        ];

        for (int i = 0; i < testData.Length; i++)
        {
            for (int j = 0; j < testData.Length; j++)
            {
                BytesHelper.Equals(testData[i], testData[j]).Should().Be(i == j);
            }
        }
    }
}