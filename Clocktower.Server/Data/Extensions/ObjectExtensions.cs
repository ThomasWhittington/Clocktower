namespace Clocktower.Server.Data.Extensions;

public static class ObjectExtensions
{
    extension(bool o)
    {
        public string ToLowerString()
        {
            return o.ToString().ToLower();
        }
    }
}