using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Z_Apps.Models.Stories;
using Z_Apps.Models.Stories.Stories;
using Z_Apps.Util;


namespace Z_Apps.Controllers
{
    [Route("api/[controller]")]
    public class StoriesController : Controller
    {
        private StoriesService storiesService;
        public StoriesController()
        {
            this.storiesService = new StoriesService();
        }


        [HttpGet("[action]/{storyId?}")]
        public IEnumerable<Story> GetOtherStories(int storyId)
        {
            if (storyId > 0)
            {
                return storiesService.GetOtherStories(storyId);
            }
            else
            {
                return null;
            }
        }
    }
}