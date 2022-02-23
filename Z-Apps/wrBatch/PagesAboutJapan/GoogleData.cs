using System.Collections;
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
using System.Web;

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
                    IEnumerable<string> wordsToRegister;
                    try
                    {
                        wordsToRegister = GetWordsToRegister();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error occurred in GetWordsToRegister method: " + ex);
                    }

                    foreach (var word in wordsToRegister)
                    {
                        try
                        {
                            await RegisterRelatedPages(word);
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.InsertErrorLog($"Error occurred in RegisterRelatedPages method. Word:{word}, Exception:{ex}");
                            await Task.Delay(1000 * 60 * 10); // 10分待機
                        }
                        await Task.Delay(1000 * 60);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.InsertErrorLog(
                        "Exception occurred in setPagesData method: " + ex
                    );
                }
                await Task.Delay(1000 * 60 * 60 * 24); // １日待機（wordsToRegisterの状況が変わるまで時間がかかるため）
            }
        }

        private static async Task RegisterRelatedPages(string word)
        {
            // Wikipedia APIからカテゴリの取得
            var category = await GetCategory(word);
            if (string.IsNullOrEmpty(category))
            {
                return;
            }

            // Google APIから関連サイトの取得
            var googleResultItems = (await GetGoogleResult(word)).items;
            if (googleResultItems == null || googleResultItems.Count() <= 5)
            {
                return;
            }

            // トランザクションを張り、データの登録
            new DBCon(DBCon.DBType.wiki_db).UseTransaction(execUpdate =>
                {
                    try
                    {
                        // 単語の削除・登録
                        execUpdate(
                            "delete from pajWords where word = @word;",
                            new Dictionary<string, object[]> {
                                { "@word", new object[2] { SqlDbType.NVarChar, word } },
                            }
                        );
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

                        // 単語に対するGoogle検索結果の削除・登録
                        execUpdate(
                            "delete from pajRelatedPages where relatedWord = @relatedWord;",
                            new Dictionary<string, object[]> {
                                { "@relatedWord", new object[2] { SqlDbType.NVarChar, word } },
                            }
                        );
                        foreach (var article in googleResultItems)
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
                        ErrorLog.InsertErrorLog(
                            "Error occurred in the DB transation: " + ex
                        );
                        return false;
                    }
                },
                60 * 60 // トランザクションのタイムアウト１時間
            );
        }

        private static async Task<GoogleResult> GetGoogleResult(string word)
        {
            try
            {
                var url = GetGoogleUrlWithParam(word);

                //15分に１回実行（Custom Search APIの上限が１日１００クエリのため）
                await Task.Delay(1000 * 60 * 15);
                var json = await Fetch.GetAsync(url);

                return JsonConvert.DeserializeObject<GoogleResult>(json);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error occurred in GetGoogleResult method:" + ex
                );
            }
        }

        private static string GetGoogleUrlWithParam(string word)
        {
            try
            {
                string url = "https://www.googleapis.com/customsearch/v1";

                //POST送信する文字列を作成
                string encodedWord =
                            HttpUtility
                            .UrlEncode(word, Encoding.GetEncoding("UTF-8"));

                return url +
                            "?key=" +
                            PrivateConsts.GOOGLE_SEARCH_API_KEY +
                            "&cx=" +
                            PrivateConsts.GOOGLE_SEARCH_ENGINE_ID +
                            "&q=" +
                            encodedWord;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Exception occurred in GetGoogleUrlWithParam method: " + ex
                );
            }
        }


        private static IEnumerable<string> GetCategoriesFromXML(string xml)
        {
            XElement xmlTree = null;
            try
            {
                xmlTree = XElement.Parse(xml);
                var query = xmlTree.Elements().FirstOrDefault(e => e.Name == "query");
                var pages = query.Elements().FirstOrDefault(e => e.Name == "pages");
                var page = pages.Elements().FirstOrDefault(e => e.Name == "page");
                var categories = page.Elements().FirstOrDefault(e => e.Name == "categories");

                if (categories == null)
                {
                    // no category
                    return new string[] { };
                }

                return categories
                    .Elements()
                    .Select(c => c.Attribute("title").Value.Replace("Category:", ""));
            }
            catch (Exception ex)
            {
                if (xmlTree == null)
                {
                    throw new Exception($"XML:{xml}, xmlTree:{xmlTree}, {ex.ToString()}");
                }
                else
                {
                    throw new Exception($"XML:{xml}, xmlTree:{xmlTree}, xmlTree.Elements():{xmlTree.Elements()}, Exception:{ex.ToString()}");
                }
            }
        }

        private static IEnumerable<string> FilterCategories(
    IEnumerable<string> categories, IEnumerable<CategoryCount> categoriesCount)
        {
            return categories
                .Where(c =>
                    {
                        string lower = c.ToLower();
                        return lower.Contains("japan") &&
                                !lower.Contains("articles") &&
                                !lower.Contains("cs1") &&
                                !lower.Contains("use") &&
                                !lower.Contains("using") &&
                                !lower.Contains("ambiguation") &&
                                !lower.Contains("wikidata");
                    }
                )
                .OrderByDescending(c =>
                {
                    var categoryCount = categoriesCount
                                            .FirstOrDefault(cc => cc.category == c);
                    if (categoryCount == null)
                    {
                        return 0;
                    }
                    return categoryCount.count;
                });
        }

        private static string RemoveNumber(
            IEnumerable<string> filteredCategories)
        {
            return filteredCategories.FirstOrDefault(c => !c.Contains("0") &&
                        !c.Contains("1") &&
                        !c.Contains("2") &&
                        !c.Contains("3") &&
                        !c.Contains("4") &&
                        !c.Contains("5") &&
                        !c.Contains("6") &&
                        !c.Contains("7") &&
                        !c.Contains("8") &&
                        !c.Contains("9"));
        }

        private static async Task<string> GetCategory(string word)
        {
            var categoriesCount = GetCategoriesCount();

            var encodedWord = HttpUtility
                        .UrlEncode(word, Encoding.GetEncoding("UTF-8"));

            var xml = await Fetch.GetAsync(
                $"https://en.wikipedia.org/w/api.php?format=xml&action=query&prop=categories&titles={encodedWord}"
            );

            var categories = GetCategoriesFromXML(xml);

            var filteredCategories = FilterCategories(categories, categoriesCount);

            var categoryWithoutNumber = RemoveNumber(filteredCategories);

            if (categoryWithoutNumber != null)
            {
                // 数値を含まないものがあれば、そちらを優先
                return categoryWithoutNumber;
            }
            return filteredCategories.FirstOrDefault();
        }

        public class CategoryCount
        {
            public string category { get; set; }
            public int count { get; set; }
        }
        private static IEnumerable<CategoryCount> GetCategoriesCount()
        {
            try
            {
                return new DBCon(DBCon.DBType.wiki_db)
                        .ExecuteSelect(@"
select category, count(word) as cnt
from pajWords
group by category
order by cnt desc;"
                        )
                        .Select(r => new CategoryCount()
                        {
                            category = (string)r["category"],
                            count = (int)r["cnt"]
                        });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "An exception occurred in GetCategoriesCount method: " + ex
                );
            }
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

            // 未登録の単語を先に
            return japaneseWords
                    .OrderBy(w =>
                        finishedWords.Contains(w)
                            ? 1
                            : 0
                    );
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
                ErrorLog.InsertErrorLog(ex.ToString());
            }
            return null;
        }

        private static IEnumerable<string> GetAlreadyFinishedWords()
        {
            try
            {
                return new DBCon(DBCon.DBType.wiki_db)
                        .ExecuteSelect(
                            " select word from pajWords;"
                            , null,
                            60 * 60 * 6 // タイムアウト６時間
                        )
                        .Select(r => (string)r["word"]);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "An exception occurred in GetAlreadyFinishedWords method: " + ex
                );
            }
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
