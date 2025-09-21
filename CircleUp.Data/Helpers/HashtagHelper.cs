using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CircleUp.Data.Helpers
{
    public static class HashtagHelper
    {
        public static List<string> GetHashtags(string? postText)
        {
            if (string.IsNullOrWhiteSpace(postText))
                return new List<string>();

            var hashtagPattern = new Regex(@"#\w+");
            var matches = hashtagPattern.Matches(postText)
                .Select(match => match.Value.TrimEnd('.', ',', '!', '?').ToLower())
                .Distinct()
                .ToList();

            return matches;
        }


    }
}
