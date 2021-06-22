using System.IO;

namespace Z_Apps.Models.SystemBase
{
    public class VersionService
    {
        public string GetVersion()
        {
            using (var sr = new StreamReader("./ClientApp/build/version.txt"))
            {
                return sr.ReadToEnd();
            }
        }
    }
}