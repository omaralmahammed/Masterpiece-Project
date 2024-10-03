using CodersBackEnd.DTO;
using CodersBackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CodersBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly MyDbContext _db;

        public BlogController(MyDbContext db) 
        {
            _db = db;
        }




        [HttpGet("GetAllCategory")]
        public IActionResult GetAllCategories()
        {
            var categories = _db.BlogCategories.ToList();

            return Ok(categories);
        }


        [HttpGet("GetRecentPost/{num}")]
        public IActionResult GetRecentPost(int num)
        {
            var recentBlogs = _db.Blogs
                   .OrderByDescending(b => b.DateOfPost)
                   .Take(num)
                   .ToList();

            return Ok(recentBlogs);
        }


        [HttpGet("GetBlogsByCategoryId/{categoryId}")]
        public IActionResult GetBlogsByCategoryId(int categoryId)
        {
            var blogs = _db.Blogs.Where( c => c.CategoryId == categoryId ).ToList();
                
            return Ok(blogs);
        }

        [HttpGet("GetBlogById/{blogId}")]
        public IActionResult GetBlogById(int blogId)
        {
            var blog = _db.Blogs.Find(blogId);

            return Ok(blog);
        }


        [HttpGet("GetCommentByBlogId/{blogId}")]
        public IActionResult GetCommentByBlogId(int blogId)
        {
            var comments = _db.BlogComments.Where(b => b.BlogId == blogId).ToList();

            return Ok(comments);
        }



        [HttpPost("AddCommentForBlog/{blogId}")]
        public IActionResult AddCommentForBlog([FromBody] BlogCommentDTO comment, int blogId)
        {
            BlogComment newComment = new BlogComment
            {
                Name = comment.Name,
                Email = comment.Email,
                Comment = comment.Comment,
                DateOfComment = DateTime.Now,
                Status = "Pending",
                BlogId = blogId
            };

            _db.BlogComments.Add(newComment);
            _db.SaveChanges();

            return Ok(newComment);
        }
    }
}
