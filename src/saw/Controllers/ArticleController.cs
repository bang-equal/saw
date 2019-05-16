using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using saw.Models;
using saw.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace saw.Controllers
{
    [Authorize]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleRepository _repository;

        public ArticleController(IArticleRepository repository)
        {
            _repository = repository;
        }

        // GET blog/articles
        [HttpGet("blog/articles")]
        public IActionResult GetArticles()
        {
            return Ok( _repository.GetArticles().Result);        
        }

        // GET blog/articles/5
        [HttpGet("blog/articles/{id}")]
        public IActionResult GetById(int id)
        {
            return Ok( _repository.GetById(id).Result);
        }

        // POST index/content
        [HttpPost]
        public IActionResult Post([FromBody]Article a)
        {
            try
            {
                if (a == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TitleAndContentRequired.ToString());
                }
                bool itemExists = _repository.DoesItemExist(a.ArticleId).Result;
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, ErrorCode.IDInUse.ToString());
                }
                _repository.AddAsync(a);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
            }
            return Ok(a);
        }

        // PUT blog/article/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Article a)
        {
            try
            {
                if (a == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TitleAndContentRequired.ToString());
                }
                var existingArticle = _repository.GetById(id);
                if (existingArticle == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _repository.UpdateAsync(a);
                
            }
            catch (Exception e)
            {
                return BadRequest(ErrorCode.CouldNotUpdateItem.ToString());
            }
            return NoContent();
        }

        // DELETE blog/article/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
             try
            {
                var article = _repository.GetById(id).Result;
                if (article == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _repository.DeleteAsync(article);
            }
            catch (Exception e)
            {
                return BadRequest(ErrorCode.CouldNotDeleteItem.ToString());
            }
            return NoContent();
        }

    public enum ErrorCode
    {
        TitleAndContentRequired,
        IDInUse,
        RecordNotFound,
        CouldNotCreateItem,
        CouldNotUpdateItem,
        CouldNotDeleteItem
    }
    }
}
