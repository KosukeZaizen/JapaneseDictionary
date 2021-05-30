using System;
using System.Threading.Tasks;
using Z_Apps.Models;
using Z_Apps.Util;

namespace Z_Apps.wrBatch
{
    public class Batch
    {
        public static void runAsync()
        {
            startBat(
                ApiCache.DeleteOldCache,
                1000 * 60 * 60 * 24, // 毎日実行
                1000 * 60 * 60 * 5 //デプロイ後５時間待機
            );

            startBat(
                SitemapCountManager.setSitemapCount,
                1000 * 60 * 60 * 3 // ３時間に１回実行
            );

            // 内部で無限ループするため、startBat関数は用いない。
            // 内部のループで、15分に１回実行（Custom Search APIの上限が１日１００クエリのため）
            Task.Run(GoogleData.setPagesData);
        }

        private static void startBat(Action action, int interval, int delay = 0)
        {
            Action start = async () =>
            {
                await Task.Delay(delay);

                while (true)
                {
                    try
                    {
                        try
                        {
                            var t = Task.Run(() =>
                            {
                                action();
                            });

                            await Task.Delay(interval);
                        }
                        catch (Exception ex)
                        {// エラーログ書き込み
                            ErrorLog.InsertErrorLog(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        // エラーログ書き込みでエラーが起きた場合は、何もしない
                    }
                }
            };
            Task.Run(start);
        }
    }
}