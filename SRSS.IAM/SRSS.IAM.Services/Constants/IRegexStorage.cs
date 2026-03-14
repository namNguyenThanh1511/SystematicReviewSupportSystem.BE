namespace SRSS.IAM.Services.Constants
{
    public interface IRegexStorage
    {
        const string REGEX_PHONE_VN_SIMPLE = @"^((\+84|84|0)(3[0-9]|5[0-9]|7[0-9]|8[0-9]|9[0-9]))[0-9]{7}$";
        const string REGEX_EMAIL = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    }
}
