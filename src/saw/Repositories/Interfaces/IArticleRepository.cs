using System.Collections.Generic;
using System.Threading.Tasks;
using saw.Models;

namespace saw.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task<bool> DoesItemExist(int id);

        Task<IList<Article>> GetArticles();

        Task<Article> GetById(int id);

        Task<int> AddAsync(Article article);

        Task UpdateAsync(Article article);

        Task DeleteAsync(Article article);

    }
}