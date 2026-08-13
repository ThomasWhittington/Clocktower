using System.IO.Abstractions;
using System.Text.Json;

namespace Clocktower.Server.Common.Services;

public class ScriptProvider(IFileSystem fileSystem) : IScriptProvider
{
    private readonly string _scriptsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Scripts");
    private const string InvalidScriptCode = "script.invalid";

    public async Task<Result<Script>> GetScriptAsync(ScriptSelect scriptSelect, string? json)
    {
        if (scriptSelect == ScriptSelect.Custom) return ParseCustomScript(json);
        return await LoadPredefinedScriptAsync(scriptSelect);
    }


    private static Result<Script> ParseCustomScript(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Result.Fail<Script>(ErrorKind.Invalid, InvalidScriptCode, "Custom script JSON cannot be empty");

        return DeserializeAndValidateScript(json, "Custom");
    }

    private async Task<Result<Script>> LoadPredefinedScriptAsync(ScriptSelect scriptSelect)
    {
        try
        {
            var filePath = Path.Combine(_scriptsPath, $"{scriptSelect}.json");
            if (!fileSystem.File.Exists(filePath))
                return Result.Fail<Script>(ErrorKind.NotFound, "script.not_found", $"Script file not found: {scriptSelect}");

            var json = await fileSystem.File.ReadAllTextAsync(filePath);
            var result = DeserializeAndValidateScript(json, scriptSelect.ToString());

            return result;
        }
        catch (Exception ex)
        {
            return Result.Fail<Script>(ErrorKind.Unexpected, "script.unexpected", $"Error loading script {scriptSelect}: {ex.Message}");
        }
    }


    private static Result<Script> DeserializeAndValidateScript(string json, string scriptName)
    {
        try
        {
            var scriptArray = JsonSerializer.Deserialize<JsonElement[]>(json);
            if (scriptArray == null || scriptArray.Length == 0)
                return Result.Fail<Script>(ErrorKind.Invalid, InvalidScriptCode, $"{scriptName} script array is empty");

            var metaElement = scriptArray[0];

            if (metaElement.ValueKind != JsonValueKind.Object ||
                !metaElement.TryGetProperty("id", out var idProp) ||
                idProp.GetString() != "_meta")
                return Result.Fail<Script>(ErrorKind.Invalid, InvalidScriptCode, $"{scriptName} script missing metadata element with id '_meta' as the first entry");

            var metaName = metaElement.GetProperty("name").GetString() ?? "";
            var metaAuthor = metaElement.GetProperty("author").GetString() ?? "";
            var characters = scriptArray.Skip(1).Select(e => e.GetString() ?? "").ToList();
            var scriptImport = new ScriptImport(metaName, metaAuthor, characters);

            return ProcessScriptImport(scriptImport);
        }
        catch (Exception ex)
        {
            return Result.Fail<Script>(ErrorKind.Invalid, "script.invalid", $"Error parsing {scriptName} script: {ex.Message}");
        }
    }

    private static Result<Script> ProcessScriptImport(ScriptImport scriptImport)
    {
        var allRoles = Role.AllRoles.ToList();

        var scriptRoles = new List<Role>();
        var failedCharacters = new List<string>();

        foreach (var characterId in scriptImport.Characters)
        {
            var thisRole = allRoles.FirstOrDefault(o => o.Id == characterId);
            if (thisRole == null)
                failedCharacters.Add(characterId);
            else
                scriptRoles.Add(thisRole);
        }

        if (failedCharacters.Count != 0)
            return Result.Fail<Script>(ErrorKind.Invalid, InvalidScriptCode, $"Characters with IDs '{string.Join(", ", failedCharacters)}' not found in role list");

        var script = new Script(scriptImport.Name, scriptImport.Author, scriptRoles);
        return Result.Ok(script);
    }
}