namespace MultiFileStream.Helpers.Extensions
{
    using System;
    using System.Text.RegularExpressions;

    internal static class MatchExtensions
    {
        public static string FindGroup(this Match match, string group)
        {
            return match.Success && match.Groups[group].Success ? match.Groups[group].Value : null;
        }

        public static T FindGroup<T>(this Match match, string group, T defaultValue = default(T))
        {
            string value = match.FindGroup(group);
            return value != null ? (T)Convert.ChangeType(value, typeof(T)) : defaultValue;
        }
    }
}
