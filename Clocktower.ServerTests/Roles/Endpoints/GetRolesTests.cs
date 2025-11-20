using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Roles.Endpoints;
using Clocktower.Server.Roles.Services;

namespace Clocktower.ServerTests.Roles.Endpoints;

[TestClass]
public class GetRolesTests
{
    private Mock<IRolesService> _mockRolesService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRolesService = new Mock<IRolesService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetRoles.Map(builder);

        var endpoint = builder.GetEndpoint("/");

        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("getRolesApi");
        endpoint.ShouldHaveSummary("Return all roles");
        endpoint.ShouldHaveDescription("Filter roles by edition and/or role type");
    }

    [TestMethod]
    public void Handle_ReturnsOkRoles()
    {
        var dummyRoles = TestDataProvider.GetDummyRoles().ToArray();
        var filterRoleType = It.IsAny<RoleType>();
        var filterEdition = It.IsAny<Edition>();
        var request = new GetRoles.Request(filterRoleType, filterEdition);
        _mockRolesService.Setup(o => o.GetRoles(filterEdition, filterRoleType)).Returns(dummyRoles);

        var result = GetRoles.Handle(request, _mockRolesService.Object);

        _mockRolesService.Verify(o => o.GetRoles(filterEdition, filterRoleType), Times.Once);

        var okResponse = result.Should().BeOfType<Ok<GetRoles.Response>>().Subject;
        okResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var response = okResponse.Value.Should().BeOfType<GetRoles.Response>().Subject;
        response.Roles.Should().BeEquivalentTo(dummyRoles);
    }
}