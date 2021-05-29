using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Z_Apps.Util;
using System.Web;

namespace Z_Apps.Models.SystemBase
{
    public class SiteMapService
    {
        public SiteMapService()
        {
            // デプロイ直後にサイトマップをキャッシュ
            Task.Run(() => { GetSiteMapText(); });
        }

        public string GetSiteMapText()
        {
            return CacheSitemap();
        }

        private string CacheSitemap()
        {
            // サイトマップ取得元を複数あるため、
            // キャッシュを統一するためにこのprivate関数を挟む
            return ApiCache.UseCache<string>(
                    "p",
                    _GetSiteMapText
                );
        }

        private string _GetSiteMapText()
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
    }
}