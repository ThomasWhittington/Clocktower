namespace Clocktower.Server.Data;

public record Script(string Name, string Author, IEnumerable<Role> Roles);