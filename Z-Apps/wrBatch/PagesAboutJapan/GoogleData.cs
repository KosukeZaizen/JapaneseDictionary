using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Z_Apps.Models;
using Z_Apps.Util;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data;
using System.Text;

namespace Z_Apps.wrBatch
{

    public class GoogleData
    {
        public static async void setPagesData()
        {
            while (true)
            {
                try
                {
                    //15分に１回実行（Custom Search APIの上限が１日１００クエリのため）
                    await Task.Delay(1000 * 60 * 15);

                    var wordsToRegister = GetWordsToRegister();

                    foreach (var word in wordsToRegister)
                    {
                        try
                        {
                            await RegisterWordAndCategory(word);
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.InsertErrorLog(ex.Message);
                        }
                        finally
                        {
                            //15分に１回実行（Custom Search APIの上限が１日１００クエリのため）
                            await Task.Delay(1000 * 60 * 15);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.InsertErrorLog(ex.Message);
                }
            }
        }

        private static async Task RegisterWordAndCategory(string word)
        {
            var category = await GetCategory(word);
            var googleResult = await GetGoogleResult(word);

            new DBCon(DBCon.DBType.wiki_db).UseTransaction(execUpdate =>
                {
                    try
                    {
                        // 単語の登録
                        var wordResultCount = execUpdate(
                            @"insert into pajWords(word,category)
                            values (@word,@category);",
                            new Dictionary<string, object[]> {
                            { "@word", new object[2] { SqlDbType.NVarChar, word } },
                            { "@category", new object[2] { SqlDbType.NVarChar, category } },
                            }
                        );
                        if (wordResultCount != 1)
                        {
                            return false;
                        }

                        // 単語に対するGoogle検索結果の登録
                        foreach (var article in googleResult.items)
                        {
                            var pageResultCount = execUpdate(
                                @"insert into pajRelatedPages(pageName,relatedWord,link,explanation)
                                values (@pageName,@relatedWord,@link,@explanation);",
                                new Dictionary<string, object[]> {
                                    { "@pageName", new object[2] { SqlDbType.NVarChar, article.title } },
                                    { "@relatedWord", new object[2] { SqlDbType.NVarChar, word } },
                                    { "@link", new object[2] { SqlDbType.NVarChar, article.link } },
                                    { "@explanation", new object[2] { SqlDbType.NVarChar, article.snippet } },
                                }
                            );
                            if (pageResultCount != 1)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.InsertErrorLog(ex.Message);
                        return false;
                    }
                },
                60 * 60 // トランザクションのタイムアウト１時間
            );
        }

        private static async Task<GoogleResult> GetGoogleResult(string word)
        {
            await Task.Delay(2000);
            var url = GetGoogleUrlWithParam(word);

            var json = await Fetch.GetAsync(url);

            return JsonConvert.DeserializeObject<GoogleResult>(json);
        }

        private static string GetGoogleUrlWithParam(string word)
        {
            string url = "https://www.googleapis.com/customsearch/v1";

            //POST送信する文字列を作成
            string encodedWord =
                        System.Web
                        .HttpUtility
                        .UrlEncode(word, Encoding.GetEncoding("UTF-8"));

            return url +
                        "?key=" +
                        PrivateConsts.GOOGLE_SEARCH_API_KEY +
                        "&cx=" +
                        PrivateConsts.GOOGLE_SEARCH_ENGINE_ID +
                        "&q=" +
                        encodedWord;
        }

        private static async Task<string> GetCategory(string word)
        {
            var xml = await Fetch.GetAsync(
                $"https://en.wikipedia.org/w/api.php?format=xml&action=query&prop=categories&titles={word}"
            );

            XElement xmlTree = XElement.Parse(xml);
            var query = xmlTree.Elements().FirstOrDefault(e => e.Name == "query");
            var pages = query.Elements().FirstOrDefault(e => e.Name == "pages");
            var page = pages.Elements().FirstOrDefault(e => e.Name == "page");
            var categories = page.Elements().FirstOrDefault(e => e.Name == "categories");

            var category = categories
                .Elements()
                .Select(c => c.Attribute("title").Value.Replace("Category:", ""))
                .Where(c => c.ToLower().Contains("japan"))
                .OrderBy(c => c.Length)
                .FirstOrDefault();

            return category;
        }

        private static IEnumerable<string> GetWordsToRegister()
        {
            // 「Japan Info」側にある、全ての日本に関する単語を取得
            var japaneseWords = GetCachedJapanesePage();
            if (japaneseWords == null)
            {
                return null;
            }

            // 既に「Pages about Japan」側にデータ取得済みの単語を取得
            var finishedWords = GetAlreadyFinishedWords();

            // 未登録の単語
            return japaneseWords
                    .Where(w => !finishedWords.Contains(w));
        }

        private static IEnumerable<string> GetCachedJapanesePage()
        {
            try
            {
                var con = new DBCon(DBCon.DBType.wiki_db);

                var result = con.ExecuteSelect(@"
select cacheData
from AllDataCache
where cacheKey = N'WikiPages'
;"
                , null,
                60 * 60 * 6)// タイムアウト６時間
                .FirstOrDefault();

                if (result != null)
                {
                    return JsonConvert
                                .DeserializeObject<List<Page>>(
                                    (string)result["cacheData"]
                                )
                                .Where(p => p.isAboutJapan == true)
                                .Select(p => p.word);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.InsertErrorLog(ex.Message);
            }
            return null;
        }

        private static IEnumerable<string> GetAlreadyFinishedWords()
        {
            return new DBCon(DBCon.DBType.wiki_db)
                    .ExecuteSelect(
                        " select word from pajWords;"
                        , null,
                        60 * 60 * 6 // タイムアウト６時間
                    )
                    .Select(r => (string)r["word"]);
        }
    }

    public class Page
    {
        public int wordId { get; set; }
        public string word { get; set; }
        public string snippet { get; set; }
        public int referenceCount { get; set; }
        public bool? isAboutJapan
        {
            get; set;
        }
    }

    public class GoogleResult
    {
        public IEnumerable<GoogleResultItems> items { get; set; }
    }
    public class GoogleResultItems
    {
        public string link { get; set; }
        public string title { get; set; }
        public string snippet { get; set; }
    }
}
