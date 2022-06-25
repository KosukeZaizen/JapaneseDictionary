using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Z_Apps.Models;
using Z_Apps.Util;
using Z_Apps.Models.SystemBase;

namespace Z_Apps.wrBatch
{
    public class SitemapCount
    {
        public string lang { get; set; }
        public DateTime time { get; set; }
        public int category { get; set; }
        public int word { get; set; }
        public int cached { get; set; }
    }
    public class SitemapCountManager
    {
        public static List<SitemapCount> counts = new List<SitemapCount>();

        public static async void setSitemapCount()
        {
            // 廃止
            //await setEachSitemap(
            //    "en",
            //    "https://wiki.lingual-ninja.com",
            //    "RelatedArticlesCache"
            //);

            await setEachSitemap(
                "ja",
                "https://wiki-jp.lingual-ninja.com",
                "RelatedArticlesCacheJp"
            );

            // 廃止
            //setPagesAboutJapanSitemap();

            counts = counts
                        .OrderByDescending(c => c.time)
                        .Where((c, i) => i <= 60)
                        .ToList();
        }

        private static void setPagesAboutJapanSitemap()
        {
            //各ページ
            var allWords =
                new DBCon(DBCon.DBType.wiki_db)
                    .ExecuteSelect("select word, category from pajWords;");

            counts.Add(new SitemapCount()
            {
                lang = "PagesAboutJapan",
                time = Time.GetJapaneseDateTime(),
                category =
                    allWords
                        .Select(r => (string)r["category"])
                        .Distinct()
                        .Count(),
                word = allWords.Count(),
                cached = 0
            });
        }

        private static async Task setEachSitemap(string lang, string hostUrl, string table)
        {
            var sitemapEn = await fetchSitemaps(hostUrl);

            var cached = new DBCon(DBCon.DBType.wiki_db).ExecuteSelect(
                $"select count(wordId) as cnt from {table};",
                null,
                60 * 10 // １０分
            ).FirstOrDefault();

            counts.Add(new SitemapCount()
            {
                lang = lang,
                time = Time.GetJapaneseDateTime(),
                category =
                    sitemapEn.Split(hostUrl + "/category/").Length - 1,
                word =
                    sitemapEn.Split(hostUrl + "/word/").Length - 1,
                cached = (int)cached["cnt"]
            });
        }

        private static async Task<string> fetchSitemaps(string hostUrl)
        {
            var sitemap = "";
            var i = 0;
            while (true)
            {
                i++;
                var url = $"{hostUrl}/sitemap{i}.xml";
                var tmp = await Fetch.GetAsync(url);
                if (!tmp.Contains("/word/") && !tmp.Contains("/category/"))
                {
                    return sitemap;
                }
                sitemap += tmp;
                await Task.Delay(2000);
            }
        }
    }
}