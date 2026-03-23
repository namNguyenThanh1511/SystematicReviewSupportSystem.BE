using System;
using System.Text.RegularExpressions;

namespace SRSS.IAM.Services.Utils
{
    public static class DoiHelper
    {
        private static readonly Regex DoiRegex = new Regex(@"10\.\d{4,9}/[-._;()/:A-Z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Normalizes a DOI string by removing prefixes like "https://doi.org/", "doi:", etc.
        /// and converting to lowercase.
        /// </summary>
        /// <param name="doi">The raw DOI string.</param>
        /// <returns>The normalized DOI, or null if invalid or empty.</returns>
        public static string? Normalize(string? doi)
        {
            if (string.IsNullOrWhiteSpace(doi))
            {
                return null;
            }

            var trimmed = doi.Trim();

            // Handle common prefixes
            if (trimmed.StartsWith("https://doi.org/", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring("https://doi.org/".Length);
            }
            else if (trimmed.StartsWith("http://doi.org/", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring("http://doi.org/".Length);
            }
            else if (trimmed.StartsWith("doi.org/", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring("doi.org/".Length);
            }
            else if (trimmed.StartsWith("doi:", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring("doi:".Length);
            }

            // Extract using regex to be safe and remove any trailing noise
            var match = DoiRegex.Match(trimmed);
            if (match.Success)
            {
                return match.Value.ToLowerInvariant();
            }

            // Fallback to basic cleaning if regex doesn't match perfectly but we have something
            return trimmed.ToLowerInvariant();
        }

        /// <summary>
        /// Checks if a string is a valid-looking DOI.
        /// </summary>
        public static bool IsValidDoi(string? doi)
        {
            if (string.IsNullOrWhiteSpace(doi)) return false;
            var normalized = Normalize(doi);
            return normalized != null && DoiRegex.IsMatch(normalized);
        }
    }
}
