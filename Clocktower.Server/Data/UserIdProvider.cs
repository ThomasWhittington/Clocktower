using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Data;

[ExcludeFromCodeCoverage(Justification = "Standard user id provider, cannot mock")]
public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}