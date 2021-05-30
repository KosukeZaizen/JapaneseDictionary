using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Z_Apps.Models;
using Z_Apps.wrBatch;

namespace Z_Apps.Controllers
{

    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        public class WikiLogData
        {
            public string procType { get; set; }
            public string term { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
        }

        [HttpGet("[action]")]
        public IEnumerable<WikiLogData> GetWikiLog()
        {
            var con = new DBCon(DBCon.DBType.wiki_db);
            string sql = @"
SELECT
    procType,
    term,
    CONVERT(VARCHAR, startTime, 120) as startTime,
    CONVERT(VARCHAR, endTime, 120) as endTime
FROM LastTopUpdate;
            ";

            var data = con.ExecuteSelect(sql, null);

            var result = data.Select(r => new WikiLogData()
            {
                procType = (string)r["procType"],
                term = (string)r["term"],
                startTime = (string)r["startTime"],
                endTime = (string)r["endTime"],
            });

            return result;
        }

        [HttpGet("[action]")]
        public IEnumerable<SitemapCount> GetSitemapCount()
        {
            return SitemapCountManager.counts;
        }
    }
}
