using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Z_Apps.Models;
using Z_Apps.Util;

namespace Z_Apps.Controllers
{
    [Route("api/[controller]")]
    public class PagesAboutJapanController : Controller
    {
        public class Category
        {
            public string category { get; set; }
            public List<string> words { get; set; }

        }

        [HttpGet("[action]")]
        public IEnumerable<Category> GetTopData()
        {
            return GetAllCategoriesWithWords();
        }

        [HttpGet("[action]/{word}")]
        public Category GetSameCategoryWords(string word)
        {
            return GetAllCategoriesWithWords()
                .FirstOrDefault(c => c.words.Contains(word));
        }

        private IEnumerable<Category> GetAllCategoriesWithWords()
        {
            return ApiCache
                .UseCache("p", () =>
                {
                    return new DBCon(DBCon.DBType.wiki_db)
                        .ExecuteSelect(
                            "select word, category from pajWords;"
                        )
                        .Aggregate(new List<Category>(), (accCat, r) =>
                        {
                            var word = (string)r["word"];
                            var category = (string)r["category"];

                            var targetCat = accCat.FirstOrDefault(
                                c => c.category == category
                            );

                            if (targetCat == null)
                            {
                                accCat.Add(new Category()
                                {
                                    category = category,
                                    words = new List<string>() { word }
                                });
                            }
                            else
                            {
                                targetCat.words.Add(word);
                            }
                            return accCat;
                        })
                        .OrderByDescending(c => c.words.Count());
                }
            );
        }

        public class RelatedPage
        {
            public string pageName { get; set; }
            public string relatedWord { get; set; }
            public string link { get; set; }
            public string explanation { get; set; }
        }

        [HttpGet("[action]/{word}")]
        public IEnumerable<RelatedPage> GetPageData(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new List<RelatedPage>();
            }

            return new DBCon(DBCon.DBType.wiki_db)
                        .ExecuteSelect(@"
                            select pageName, relatedWord, link, explanation
                            from pajRelatedPages
                            where relatedWord = @relatedWord;",
                            new Dictionary<string, object[]> {
                                { "@relatedWord", new object[2] { SqlDbType.NVarChar, word } }
                            }
                        )
                        .Select(r => new RelatedPage()
                        {
                            pageName = (string)r["pageName"],
                            relatedWord = (string)r["relatedWord"],
                            link = (string)r["link"],
                            explanation = (string)r["explanation"]
                        });
        }
    }
}
