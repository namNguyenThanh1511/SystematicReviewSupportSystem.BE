using SRSS.IAM.Services.Attributes;
using System.Text.RegularExpressions;

namespace SRSS.IAM.Services.Helper
{
    public static class ValidationHelper
    {
        public static bool IsEmail(string input)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, emailPattern);
        }

        public static bool IsVietnamesePhone(string input)
        {
            var phoneAttribute = new PhoneVNAttribute();
            return phoneAttribute.IsValid(input);
        }

        public static bool IsPhoneNumber(string input)
        {
            string phonePattern = @"^\+?\d{10,15}$";
            return Regex.IsMatch(input, phonePattern);
        }

        // Format phone để gửi OTP: 0937634111 -> 84937634111
        public static string FormatPhoneForOTP(string phoneNumber)
        {
            string cleanPhone = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

            if (cleanPhone.StartsWith("0"))
            {
                return "84" + cleanPhone.Substring(1);
            }
            return cleanPhone;
        }

        public static string CleanPhoneForDB(string phoneNumber)
        {
            return phoneNumber.Trim().Replace(" ", "").Replace("-", "");
        }
    }
}
