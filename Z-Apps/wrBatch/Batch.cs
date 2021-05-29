using System;
using System.Threading.Tasks;
using Z_Apps.Models;
using Z_Apps.Util;

namespace Z_Apps.wrBatch
{
    public class Batch
    {
        public static async void runAsync()
        {

            await Task.Delay(1000 * 60 * 60 * 5);//デプロイ後５時間待機

            while (true)
            {
                try
                {

                    var t = Task.Run(() =>
                    {
                        // MakeDbBackupAsync();
                        // DeleteOpeLogs();
                        ApiCache.DeleteOldCache();
                    });

                    await Task.Delay(1000 * 60 * 60 * 24);//１日待機

                }
                catch (Exception ex)
                {
                    ErrorLog.InsertErrorLog(ex.Message);
                }
            }
        }
    }
}