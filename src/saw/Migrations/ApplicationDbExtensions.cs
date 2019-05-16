using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using saw.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace saw.Migrations
{
    public static class ApplicationDbExtensions
    {

        public static void EnsureSeedData(this ApplicationDbContext context, string ArticleData)
        {
            List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(ArticleData);

            if (!context.Database.GetPendingMigrations().Any())
            {

                if (!context.Articles.Any())
                {
                    foreach(Article a in articles)
                    {
                        context.Articles.AddRange(
                            new Article {
                            ArticleId = a.ArticleId, 
                            ArticleTitle = a.ArticleTitle,
                            ArticleText = a.ArticleText
                            }); 
                        context.SaveChanges();
                  
                    }
                }                    
            }
        }
    }
}
