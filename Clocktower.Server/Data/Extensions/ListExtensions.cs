namespace Clocktower.Server.Data.Extensions;

public static class ListExtensions
{
    extension<T>(IEnumerable<T> identifiableArr) where T : IIdentifiable
    {
        public T? GetById(string id)
        {
            return identifiableArr.FirstOrDefault(o => o.Id == id);
        }
    }
}