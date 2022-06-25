using System.Collections.Generic;

namespace Z_Apps.Models.SystemBase
{
    public class AppInfo
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class Apps
    {
        // アプリ追加時は、hostsに新しいドメインを追加する
        public static readonly List<AppInfo> apps = new List<AppInfo>()
        {
            new AppInfo(){
                Key = "dictionary",
                Title = "Japanese Dictionary",
                Description ="Free website to learn the meaning of Japanese words! You can learn a lot of Japanese words!",
            },
            //new AppInfo(){
            //    Key = "japan",
            //    Title = "Pages about Japan",
            //    Description ="Website to introduce articles about Japan. You can learn about Japan with many topics.",
            //},
            new AppInfo(){
                Key = "admin",
                Title = "Admin page",
                Description ="Admin for me",
            },
        };
    }
}