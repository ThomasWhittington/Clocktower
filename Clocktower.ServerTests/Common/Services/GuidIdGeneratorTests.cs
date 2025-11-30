using Clocktower.Server.Common.Services;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class GuidIdGeneratorTests
{
    [TestMethod]
    public void GenerateId_Generates_RandomString()
    {
        var sut = new GuidIdGenerator();
        const int repeat = 1000;
        var ids = new List<string>();
        for (int i = 0; i < repeat; i++)
        {
            var newId = sut.GenerateId();
            ids.Add(newId);
        }
        
        CollectionAssert.AllItemsAreUnique(ids);
    }
}