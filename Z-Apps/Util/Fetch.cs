using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Z_Apps.Models;

namespace Z_Apps.Util
{
    public class Fetch
    {
        public static async Task<string> GetAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.InsertErrorLog(ex.Message);
                return "";
            }
        }

        public static string PostAsync(
            string url, Dictionary<string, string> param = null
        )
        {
            try
            {
                if (param == null)
                {
                    param = new Dictionary<string, string>() { };
                }

                using (var client = new HttpClient())
                {
                    //文字コードを指定する
                    System.Text.Encoding enc =
                        System.Text.Encoding.GetEncoding("UTF-8");

                    //POST送信する文字列を作成
                    string postData =
                        string.Join("&", param.Select((p) => $"{p.Key}={p.Value}"));

                    //バイト型配列に変換
                    byte[] postDataBytes = enc.GetBytes(postData);

                    System.Net.WebClient wc = new System.Net.WebClient();
                    //ヘッダにContent-Typeを加える
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    //データを送信し、受信する
                    byte[] resData = wc.UploadData(url, postDataBytes);
                    wc.Dispose();

                    //受信したデータを表示する
                    return enc.GetString(resData);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.InsertErrorLog(ex.Message);
                return "";
            }
        }

    }
}