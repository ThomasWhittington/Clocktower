namespace Clocktower.Server.Data.Extensions;

public static class ModelExtensions
{
    extension(IEnumerable<IIdentifiable> identifiables)
    {
        public IEnumerable<string> GetIds() => identifiables.Select(o => o.Id);
    }
}