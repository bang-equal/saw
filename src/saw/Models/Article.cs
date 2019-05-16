using System.Collections;
using System.Collections.Generic;
using saw.Repositories.Interfaces;

namespace saw.Models
{
    public class Article
    {
        public int ArticleId { get; set; }
        public string ArticleTitle { get; set; } = "";
        public string ArticleText { get; set; } = "";
    }  
}