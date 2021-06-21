using System.Net.Http;
using System.Threading.Tasks;

namespace Z_Apps.Models.SystemBase
{
    public class VersionService
    {
        public async Task<string> GetVersion(string host)
        {
            string resultText = "";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://" + host + "/version.txt");
                resultText = await response.Content.ReadAsStringAsync();
            }
            return resultText;
        }
    }
}