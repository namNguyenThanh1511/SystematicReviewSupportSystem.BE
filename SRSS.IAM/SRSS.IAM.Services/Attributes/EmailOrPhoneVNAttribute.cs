using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.Attributes
{
    public class EmailOrPhoneVNAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Email or phone number is required");
            }

            string input = value.ToString()!.Trim();

            // Check if it's a valid email
            if (IsValidEmail(input))
            {
                return ValidationResult.Success;
            }

            // Check if it's a valid Vietnamese phone number using existing logic
            var phoneValidator = new PhoneVNAttribute();
            var phoneValidationResult = phoneValidator.IsValid(input);

            if (phoneValidationResult)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Số điện thoại khônh đúng định dạng VN");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
