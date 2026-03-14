using SRSS.IAM.Services.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SRSS.IAM.Services.Attributes
{
    public class PhoneVNAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string phone = value.ToString()!.Trim().Replace(" ", "").Replace("-", "");
            string pattern = IRegexStorage.REGEX_PHONE_VN_SIMPLE;

            if (!Regex.IsMatch(phone, pattern))
            {
                return new ValidationResult("Định dạng số điện thoại phải là Việt Nam");
            }

            return ValidationResult.Success;
        }
    }
}
