using System.Reflection;

namespace SRSS.IAM.Repositories
{
    public class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}
