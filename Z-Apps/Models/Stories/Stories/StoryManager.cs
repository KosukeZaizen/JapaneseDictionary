using System.Collections.Generic;

namespace Z_Apps.Models.Stories.Stories
{
    public class StoryManager
    {
        private readonly DBCon Con;
        public StoryManager()
        {
            Con = new DBCon(DBCon.DBType.z_apps);
        }

        public IEnumerable<Story> GetAllStories()
        {
            //SQL文作成
            string sql = @"
            select * from tblStoryMst
            where Released = 1
            order by [Order] desc";

            //List<Dictionary<string, Object>>型で取得
            var stories = Con.ExecuteSelect(sql, null);

            //Story型に変換してreturn
            var resultStories = new List<Story>();
            foreach (var dicStory in stories)
            {
                var story = new Story();
                story.StoryId = (int)dicStory["StoryId"];
                story.StoryName = (string)dicStory["StoryName"];
                story.Description = (string)dicStory["Description"];
                story.Order = (int?)dicStory["Order"];

                resultStories.Add(story);
            }
            return resultStories;
        }
    }
}
