using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Z_Apps.Util;
using System.Web;

namespace Z_Apps.Models.SystemBase
{
    public class SiteMapService
    {
        private readonly StorageService storageService;
        private readonly StorageBackupService storageBkService;

        public SiteMapService(StorageService storageService, StorageBackupService storageBkService)
        {
            this.storageService = storageService;
            this.storageBkService = storageBkService;
        }

        public async Task<IEnumerable<Dictionary<string, string>>> GetSiteMap(bool onlyStrageXmlFile = false)
        {
            var listResult = new List<Dictionary<string, string>>();

            var resultXML = await GetSiteMapText(onlyStrageXmlFile);

            XElement xmlTree = XElement.Parse(resultXML);
            var urls = xmlTree.Elements();

            foreach (XElement url in urls)
            {
                var dic = new Dictionary<string, string>();
                dic.Add("loc", url.Elements().Where(u => u.Name.ToString().Contains("loc")).First().Value);
                dic.Add("lastmod", url.Elements().Where(u => u.Name.ToString().Contains("lastmod")).First().Value);

                listResult.Add(dic);
            }

            return listResult;
        }


        public async Task<string> GetSiteMapText(bool onlyStrageXmlFile = false)
        {
            return await ApiCache.UseCacheAsync(
                "Z_Apps.Models.SystemBase.SiteMapService",
                "GetSiteMapText",
                onlyStrageXmlFile ? "true" : "false",
                async () =>
                {
                    return await _GetSiteMapText(onlyStrageXmlFile);
                });
        }


        public async Task<string> _GetSiteMapText(
            bool onlyStrageXmlFile = false
        )
        {
            try
            {

                var lstSitemap = new List<Dictionary<string, string>>();

                //------------------------------------------------------------

                //top page (noindexのためコメントアウト)
                var dic1 = new Dictionary<string, string>();
                dic1["loc"] = "https://dictionary.lingual-ninja.com";
                lstSitemap.Add(dic1);

                //各ページ
                var wikiService = new WikiService();
                IEnumerable<string> allWords = wikiService.GetAllWordsFromDB(0);
                foreach (string word in allWords)
                {
                    var encodedWord = HttpUtility
                                        .UrlEncode(word, Encoding.UTF8)
                                        .Replace("+", "%20");
                    var dicWordId = new Dictionary<string, string>();
                    dicWordId["loc"] = "https://dictionary.lingual-ninja.com/dictionary/" + encodedWord;
                    lstSitemap.Add(dicWordId);
                }

                //------------------------------------------------------------

                string partialXML = GetStringSitemapFromDics(lstSitemap);
                return (
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                            + partialXML + "</urlset>"
                    );
            }
            catch (Exception ex)
            {
                ErrorLog.InsertErrorLog(ex.Message);
            }

            return "";
        }


        private string GetStringSitemapFromDics(IEnumerable<Dictionary<string, string>> sitemapItems)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in sitemapItems)
            {
                sb.Append("<url><loc>");
                sb.Append(item["loc"]);
                sb.Append("</loc></url>");
            }

            return sb.ToString();
        }

        public async Task<bool> RegisterSitemap(IEnumerable<Dictionary<string, string>> sitemapItems)
        {
            //backup
            var previousXML = await GetSiteMapText();
            DateTime dt = DateTime.Now;
            await storageBkService.UploadAndOverwriteFileAsync(previousXML, "lingual-storage-bk/sitemap/" + dt.ToString("yyyy-MM") + "-sitemap.xml");

            //register new sitemap
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("\n");
            sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.Append("\n");

            foreach (var item in sitemapItems)
            {
                sb.Append("  <url>");
                sb.Append("\n");

                sb.Append("    <loc>");
                sb.Append(item["loc"]);
                sb.Append("</loc>");
                sb.Append("\n");

                sb.Append("    <lastmod>");
                sb.Append(item["lastmod"]);
                sb.Append("</lastmod>");
                sb.Append("\n");

                sb.Append("  </url>");
                sb.Append("\n");
            }
            sb.Append("</urlset>");

            string strSitemap = sb.ToString();
            await storageService.UploadAndOverwriteFileAsync(strSitemap, "appsPublic/sitemap.xml");

            return true;
        }
    }
}