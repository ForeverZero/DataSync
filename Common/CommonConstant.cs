using System.IO;
using System.Reflection;

namespace DataSynchronizor.Common
{
    public class CommonConstant
    {
        public const string KeyAppName = "DataSync";
        public static readonly string AppLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string AppExePath =
            $"{AppLocation}\\DataSynchronizor.exe";
    }
}