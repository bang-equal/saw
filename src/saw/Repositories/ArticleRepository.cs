using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using saw.Models;
using saw.Repositories.Interfaces;

namespace saw.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticleRepository(ApplicationDbContext context)
        {
            _context = context;    
        }

        public async Task<bool> DoesItemExist(int id)
        {
            var dbEntity = await _context.Articles.AnyAsync(article => article.ArticleId == id);
            return dbEntity;
        }
       
        public async Task <IList<Article>> GetArticles()
        {
            var dbEntity = await _context.Articles.ToListAsync();
            return dbEntity;
        }

        public async Task <Article> GetById(int id)
        {
            var dbEntity = await _context.Articles.FirstOrDefaultAsync();
            return dbEntity;
            
        }

        public async Task <int> AddAsync(Article article)
        {
            _context.Articles.Add(new Article { ArticleTitle = article.ArticleTitle, ArticleText = article.ArticleText});
            return await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Attach(article);
            var entry = _context.Entry(article);
            entry.Property(e => e.ArticleTitle).IsModified = true;
            entry.Property(e => e.ArticleText).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Article article)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }
}