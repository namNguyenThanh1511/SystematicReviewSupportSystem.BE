using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Utils
{
	/// <summary>
	/// Helper để convert PascalCase sang snake_case cho PostgreSQL
	/// </summary>
	public static class NamingConventionHelper
	{
		/// <summary>
		/// Convert PascalCase sang snake_case
		/// Example: "SystematicReviewProject" -> "systematic_review_project"
		/// </summary>
		public static string ToSnakeCase(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return input;

			// Insert underscore before uppercase letters (except first character)
			var snakeCase = Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToLower();

			return snakeCase;
		}
	}
}