using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Z_Apps.Models;
using Z_Apps.Util;

public class WikiService
{
    private readonly List<string> WordsToExclude = new List<string>
        {
            "一覧",
            "曖昧さ",
            "人物",
            "年",
            "月",
            "邾",
            "(",
            "（",
            ".",
        };

    public IEnumerable<string> GetAllWordsFromDB(int num)
    {
        var con = new DBCon(DBCon.DBType.wiki_db);

        var top = num == 0 ? "" : "top(@num)";
        var sql = @$"
            select {top} word from ZAppsDictionaryCache
            with(index(noindex))
            where noindex = 0
            order by word desc;";

        var result = con.ExecuteSelect(
                sql,
                new Dictionary<string, object[]> { { "@num", new object[2] { SqlDbType.Int, num } } },
                60 * 60 * 2 // 2 hours
            );

        return result.Select(r => (string)r["word"]);
    }


    [DataContract]
    class Data
    {
        [DataMember]
        public int? wordId { get; set; }

        [DataMember]
        public string snippet { get; set; }
    }
    class DictionaryResult
    {
        public int? wordId { get; set; }
        public string snippet { get; set; }
        public string xml { get; set; }
        public string translatedWord { get; set; }
    }
    public class CacheResult
    {
        public string Response { get; set; }
        public bool Noindex { get; set; }
    }
    public async Task<CacheResult> GetEnglishWordAndSnippet(string word)
    {
        var con = new DBCon(DBCon.DBType.wiki_db);

        Func<Task<DictionaryResult>> getDictionaryDataWithoutCache = async () =>
        {
            try
            {
                Data w = null;
                using (var client = new HttpClient())
                {
                    string json = "";
                    var targetUrl = "https://wiki-jp.lingual-ninja.com/api/WikiWalks/GetWordIdAndSnippet?word=" + word;
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(targetUrl);
                        json = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.InsertErrorLog($"Exception occurred when fetching. URL:{targetUrl}, Exception:{ex.ToString()}");
                        return null;
                    }

                    var serializer = new DataContractJsonSerializer(typeof(Data));
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        try
                        {
                            w = (Data)serializer.ReadObject(ms);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"An exception occurred in (Data)serializer.ReadObject(ms). Word:{word}, Exception{ex}");
                        }

                        if (w == null || w.wordId == null)
                        {
                            return new DictionaryResult() { xml = "", translatedWord = "", wordId = 0, snippet = "" };
                        }

                        try
                        {
                            //英語に翻訳
                            w.snippet = (
                                await MakeEnglish(
                                    w.snippet
                                        .Replace("<bold>", "")
                                        .Replace("<bold", "")
                                        .Replace("<bol", "")
                                        .Replace("<bo", "")
                                        .Replace("<b", "")
                                        .Replace("</bold>", "")
                                        .Replace("</bold", "")
                                        .Replace("</bol", "")
                                        .Replace("</bo", "")
                                        .Replace("</b", "")
                                        .Replace("</", "")
                                        .Replace("/bold>", "")
                                        .Replace("bold>", "")
                                        .Replace("old>", "")
                                        .Replace("ld>", "")
                                        .Replace("d>", "")
                                        .Replace("#", "")
                                        .Replace("?", "")
                                        .Replace("&", "")
                                )
                            );
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.InsertErrorLog($"Exception occurred when fetching google translation app. Word:{word}, Exception:{ex.ToString()}");
                            return null;
                        }
                    }
                }

                var xml = "";
                try
                {
                    xml = GetFurigana(word);
                }
                catch (Exception ex)
                {
                    ErrorLog.InsertErrorLog(ex.ToString());
                    return null;
                }

                //受信したデータを表示する
                return new DictionaryResult()
                {
                    xml = xml,
                    wordId = w.wordId,
                    snippet = w.snippet,
                    translatedWord = await MakeEnglish(word
                            .Replace("#", "")
                            .Replace("?", "")
                            .Replace("&", ""))
                };
            }
            catch (Exception ex)
            {
                ErrorLog.InsertErrorLog($"Exception occurred in getDictionaryDataWithoutCache. Target word:{word}, Exception:{ex.ToString()}");
            }
            return null;
        };


        //キャッシュ取得
        var cache = con.ExecuteSelect(
                        "select response, noindex from ZAppsDictionaryCache where word = @word",
                        new Dictionary<string, object[]> {
                            { "@word", new object[2] { SqlDbType.NVarChar, word } }
                        }
                    ).FirstOrDefault();

        if (cache != null)
        {
            //キャッシュデータあり
            return new CacheResult()
            {
                Response = (string)cache["response"],//jsonもしくは「removed」という文字列
                Noindex = (bool)cache["noindex"]
            };
        }
        else
        {
            //キャッシュデータなし
            DictionaryResult obj;
            string json;
            if (WordsToExclude.Any(ew => word.Contains(ew)))
            {
                //除外対象文字列を含む場合
                obj = new DictionaryResult() { xml = "", translatedWord = "", wordId = 0, snippet = "" };
                json = "removed";
            }
            else
            {
                //通常時
                obj = await getDictionaryDataWithoutCache();
                if (obj.xml == "")
                {
                    // Probably the Kanji is not valid Japanese.
                    // Example: 庾亮, 李載佾
                    ErrorLog.InsertErrorLog($"XML from next.js furigana api is an empty string. Word:{word}");
                    obj = new DictionaryResult() { xml = "", translatedWord = "", wordId = 0, snippet = "" };
                    json = "removed";
                }
                else
                {
                    json = JsonSerializer.Serialize(obj);
                }
            }

            var task = Task.Run(async () =>
            {
                //5秒待って登録
                await Task.Delay(5 * 1000);

                if ((
                    obj.wordId > 0 && obj.xml.Length > 0
                        && obj.xml.Length > 0
                        && obj.translatedWord.Length > 0
                        && json.Contains("wordId")
                    )
                    || json == "removed")
                {
                    var noindex = json == "removed" ? 1 : 0;

                    con.ExecuteUpdate($"insert into ZAppsDictionaryCache values(@word, @json, GETDATE(), {noindex});",
                        new Dictionary<string, object[]> {
                            { "@json", new object[2] { SqlDbType.NVarChar, json } },
                            { "@word", new object[2] { SqlDbType.NVarChar, word } }
                        });
                }
            });

            //上記完了を待たずにreturn
            return new CacheResult()
            {
                Response = json,
                Noindex = false
            };
        }
    }


    public string GetFurigana(string encodedWord)
    {
        var jsonString = Fetch.PostAsync(
                $"{Consts.ARTICLES_URL}/api/japaneseDictionary/yahooFuriganaV1",
                new Dictionary<string, string>() {
                        {"word", encodedWord},
                        {"yahooAppId", PrivateConsts.YAHOO_API_ID}
                }
            );

        if (jsonString == "")
        {
            throw new Exception($"Error occurred when sending POST request to next.js furigana api.");
        }

        var nextResult = JsonSerializer.Deserialize<NextResult>(jsonString);

        if (nextResult.responseType == "success")
        {
            return nextResult.xml;
        }
        throw new Exception($"Error from next.js furigana api. ResponseType: {nextResult.responseType}");
    }

    private class NextResult
    {
        public string responseType { get; set; }
        public string xml { get; set; }
    }

    public async Task<string> MakeEnglish(string kanji)
    {
        // Explanation:
        // https://qiita.com/satto_sann/items/be4177360a0bc3691fdf

        // Apps Script:
        // https://script.google.com/home/projects/1ibWrQwdxAHyvEtKbxvuU8OztGlGCumDb6VmEc59nkJ_prajItLPayLxx/edit

        string url = "https://script.google.com/macros/s/AKfycbzybNyMvQkLkgzgtxOE-8Js7dTBnECkj4uN7Q2vDMMPbXMkEoCd5grxM9RTiPstgttMIw/exec?text="
            + kanji + "&source=ja&target=en";

        using (var client = new HttpClient())
        {
            string jsonString = null;
            try
            {
                jsonString = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"An exception occurred when executing GetStringAsync. URL:${url}, Exception:${ex.ToString()}");
            }

            GoogleResult googleResult = null;
            try
            {
                googleResult =
                    JsonSerializer.Deserialize<GoogleResult>(jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"An exception occurred when deserializing. JsonString:${jsonString}, Exception:${ex.ToString()}");
            }

            if (googleResult?.code == 200)
            {
                return googleResult.text;
            }
            throw new Exception("Result code from Google Translate App is not 200, or GoogleResult is null. GoogleResult:" + googleResult);
        }
    }
    private class GoogleResult
    {
        public int code { get; set; }
        public string text { get; set; }
    }
}

