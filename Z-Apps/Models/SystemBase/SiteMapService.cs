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
        // アプリ追加時は、hostNamesに新しいドメインを追加する
        public Dictionary<string, string> hostNames = new Dictionary<string, string>(){
            {"dictionary", "dictionary.lingual-ninja.com"},
            {"local", "localhost"}
        };

        public SiteMapService()
        {
            // デプロイ直後にサイトマップをキャッシュ
            Task.Run(() =>
            {
                foreach (var (key, hostName) in hostNames)
                {
                    GetSiteMapText(hostName);
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

                if (url == hostNames["dictionary"])
                {
                    // Japanese Dictionary
                    lstSitemap = GetJapaneseDictionarySitemap();
                }
                else if (url == hostNames["local"])
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
                dicWordId["loc"] = "https://dictionary.lingual-ninja.com/dictionary/"
                                        + encodedWord;

                lstSitemap.Add(dicWordId);
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