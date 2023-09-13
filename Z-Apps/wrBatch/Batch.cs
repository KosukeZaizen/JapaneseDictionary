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
#if DEBUG
#else
            //----------------------------------------------------
            // ↓デバッグでは実行しないバッチ↓
            //----------------------------------------------------

            // API Cacheの古いデータ削除
            startBat(
                ApiCache.DeleteOldCache,
                1000 * 60 * 60 * 24, // 毎日実行
                1000 * 60 * 60 * 5 //デプロイ後５時間待機
            );
#endif

            //----------------------------------------------------
            // ↓デバッグでも実行するバッチ↓
            //----------------------------------------------------
            startBat(
                SitemapCountManager.setSitemapCount,
                1000 * 60 * 60 * 3 // ３時間に１回実行
            );
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
                    catch (Exception)
                    {
                        // エラーログ書き込みでエラーが起きた場合は、何もしない
                    }
                }
            };
            Task.Run(start);
        }
    }
}