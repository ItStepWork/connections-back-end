﻿using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : Controller
    {
        [Authorize]
        [HttpGet("GetPosts")]
        public async Task<ActionResult> GetPosts(string id)
        {
            var result = await PostService.GetPostsAsync(id);
            return Ok(result);
        }

        [HttpPost("CreatePost")]
        public async Task<ActionResult> CreatePost([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.RecipientId) || (string.IsNullOrEmpty(request.Text) && request.File == null)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.CreatePostAsync(userId, request);
            return Ok("Ok");
        }

        [HttpPost("SendComment")]
        public async Task<ActionResult> SendComment(Request request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.RecipientId) || string.IsNullOrEmpty(request.Text)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.SendCommentAsync(userId, request);
            return Ok("Ok");
        }

        [HttpPost("SetLike")]
        public async Task<ActionResult> SetLike(Request request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.RecipientId)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.SetLikeAsync(userId, request);
            return Ok("Ok");
        }

        [HttpPost("RemovePost")]
        public async Task<ActionResult> RemovePost(Request request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await PostService.GetPostAsync(request.UserId, request.Id);
            if (result == null) return Conflict("Post not found");

            if (userId == request.UserId)
            {
                await PostService.RemovePostAsync(request.UserId, request.Id);
                return Ok("Ok");
            }
            else
            {
                var group = await GroupService.GetGroupAsync(request.UserId);
                if (group == null) return Conflict("No access");
                else
                {
                    if(group.AdminId == userId)
                    {
                        await PostService.RemovePostAsync(request.UserId, request.Id);
                        return Ok("Ok");
                    }
                    else return Conflict("No access");
                }
            }
        }
    }
}