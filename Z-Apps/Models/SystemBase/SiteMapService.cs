using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Z_Apps.Util;
using System.Web;
using System.Linq;

namespace Z_Apps.Models.SystemBase
{
    public class SiteMapService
    {
        public SiteMapService()
        {
            // デプロイ直後にサイトマップをキャッシュ
            Task.Run(() =>
            {
                foreach (var app in Apps.apps)
                {
                    GetSiteMapText($"{app.Key}.lingual-ninja.com");
                }
            });
        }

        public string GetSiteMapText(string hostName)
        {
            // サイトマップ取得元を複数あるため、
            // キャッシュを統一するためにこのprivate関数を挟む
            return CacheSitemap(hostName);
        }

        private string CacheSitemap(string hostName)
        {
            return ApiCache.UseCache<string>(
                        hostName,
                        () => _GetSiteMapText(hostName)
                    );
        }

        private string _GetSiteMapText(string url)
        {
            try
            {
                List<Dictionary<string, string>> lstSitemap;

                if (url == "dictionary.lingual-ninja.com")
                {
                    lstSitemap = GetJapaneseDictionarySitemap();
                }
                else if (url == "japan.lingual-ninja.com")
                {
                    lstSitemap = GetPagesAboutJapanSitemap();
                }
                else if (url == "localhost")
                {
                    // ローカルでのデバッグ時
                    lstSitemap = GetLocalhostSitemap();
                }
                else
                {
                    // ヒットするURLが無ければ空
                    lstSitemap = new List<Dictionary<string, string>>();
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

        private List<Dictionary<string, string>> GetJapaneseDictionarySitemap()
        {
            var lstSitemap = new List<Dictionary<string, string>>();

            //top page
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
                dicWordId["loc"] = "https://dictionary.lingual-ninja.com/dictionary/"
                                        + encodedWord;

                lstSitemap.Add(dicWordId);
            }

            return lstSitemap;
        }

        private List<Dictionary<string, string>> GetPagesAboutJapanSitemap()
        {
            var lstSitemap = new List<Dictionary<string, string>>();

            //top page
            var dic1 = new Dictionary<string, string>();
            dic1["loc"] = "https://japan.lingual-ninja.com";
            lstSitemap.Add(dic1);

            //各ページ
            IEnumerable<string> allWords =
                        new DBCon(DBCon.DBType.wiki_db)
                            .ExecuteSelect(
                                "select word from pajWords;",
                                null,
                                60 * 60 * 2 // 2 hours
                            )
                            .Select(r => (string)r["word"]);

            foreach (string word in allWords)
            {
                var encodedWord = HttpUtility
                                    .UrlEncode(word, Encoding.UTF8)
                                    .Replace("+", "%20");

                lstSitemap.Add(new Dictionary<string, string>(){
                    {
                        "loc",
                        $"https://japan.lingual-ninja.com/p/{encodedWord}"
                    }
                });
            }

            return lstSitemap;
        }

        private List<Dictionary<string, string>> GetLocalhostSitemap()
        {
            var lstSitemap = new List<Dictionary<string, string>>();

            var localDic = new Dictionary<string, string>();
            localDic["loc"] = "This_is_localhost_sitemap";
            lstSitemap.Add(localDic);

            return lstSitemap;
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