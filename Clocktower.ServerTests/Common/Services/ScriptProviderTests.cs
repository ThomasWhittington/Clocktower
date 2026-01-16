using System.IO.Abstractions;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class ScriptProviderTests
{
    private Mock<IFileSystem> _mockFileSystem = null!;
    private IScriptProvider _sut = null!;

    [TestInitialize]
    public void Initialize()
    {
        _mockFileSystem = StrictMockFactory.Create<IFileSystem>();

        _sut = new ScriptProvider(_mockFileSystem.Object);
    }


    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_Custom_WhenJsonMissing()
    {
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom, null);

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Invalid, "script.invalid");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_Custom_WhenJsonInvalid()
    {
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom, "abcde");

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Invalid, "script.invalid");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_Custom_WhenJsonEmpty()
    {
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom, "[]");

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Invalid, "script.invalid");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_Custom_WhenMetaNotFound()
    {
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom, "[\"role1\"]");

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Invalid, "script.invalid");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_Custom_WhenRoleNotFound()
    {
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom,
            """
            [
                {
                  "id": "_meta",
                  "author": "AUTHOR",
                  "name": "NAME"
                },
                "role1"
            ]
            """
        );

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Invalid, "script.invalid");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsOk_Custom_WhenScriptIsValid()
    {
        var expected = new Script("NAME", "AUTHOR", [Role.Chef(), Role.Empath()]);
        var result = await _sut.GetScriptAsync(ScriptSelect.Custom,
            """
            [
                {
                  "id": "_meta",
                  "author": "AUTHOR",
                  "name": "NAME"
                },
                "chef",
                "empath"
            ]
            """
        );

        result.Should().BeOfType<Result<Script>>();
        result.ShouldSucceedWithEquivalent(expected);
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_PreDefined_WhenFileDoesNotExist()
    {
        _mockFileSystem.Setup(o => o.File.Exists(It.IsAny<string>())).Returns(false);

        var result = await _sut.GetScriptAsync(ScriptSelect.TroubleBrewing, null);

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.NotFound, "script.not_found");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsError_PreDefined_WhenReadFails()
    {
        _mockFileSystem.Setup(o => o.File.Exists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(o => o.File.ReadAllTextAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
        var result = await _sut.GetScriptAsync(ScriptSelect.TroubleBrewing, null);

        result.Should().BeOfType<Result<Script>>();
        result.ShouldFailWith(ErrorKind.Unexpected, "script.unexpected");
    }

    [TestMethod]
    public async Task GetScriptAsync_ReturnsOk_PreDefined_WhenScriptIsValid()
    {
        var expected = new Script("NAME", "AUTHOR", [Role.Chef(), Role.Empath()]);
        _mockFileSystem.Setup(o => o.File.Exists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(o => o.File.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(
            """
            [
                {
                  "id": "_meta",
                  "author": "AUTHOR",
                  "name": "NAME"
                },
                "chef",
                "empath"
            ]
            """
        );
        var result = await _sut.GetScriptAsync(ScriptSelect.TroubleBrewing, null);

        result.Should().BeOfType<Result<Script>>();
        result.ShouldSucceedWithEquivalent(expected);
    }
}