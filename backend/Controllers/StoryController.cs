﻿using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController : Controller
    {

        [HttpGet("GetStoryPhotos")]
        public async Task<ActionResult> GetStoryPhotos(string userId, string storyId)
        {
            var result = await GalleryService.GetStoryPhotosAsync(userId, storyId);
            return Ok(result);
        }

        [HttpGet("GetStories")]
        public async Task<ActionResult> GetStories(string userId)
        {
            var result = await StoryService.GetStoriesAsync(userId);
            return Ok(result);
        }

        [HttpPost("AddStory")]
        public async Task<ActionResult> AddStory([FromForm]Request request)
        {
            if (string.IsNullOrEmpty(request.Name)) return Conflict("Name is null or empty");
            if (request.Files == null || request.Files.Length == 0) return Conflict("No files selected");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            string storyId = Guid.NewGuid().ToString("N");

            foreach (var file in request.Files)
            {
                string photoId = Guid.NewGuid().ToString("N");
                var url = await UserService.SaveFileAsync(file, "Photos", photoId);
                if (url != null)
                {
                    Photo photo = new();
                    photo.Id = photoId;
                    photo.Url = url;
                    photo.StoryId = storyId;
                    await GalleryService.UpdatePhotoAsync(userId, photoId, photo);
                }
            }
            Story story = new();
            story.Id = storyId;
            story.Name = request.Name;
            story.CreatedTime = DateTime.UtcNow;
            await StoryService.UpdatStoryAsync(userId, storyId, story);
            return Ok("Ok");
        }

        [HttpDelete("DeleteStory")]
        public async Task<ActionResult> DeleteStory(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await StoryService.RemoveStoryAsync(userId, id);
            return Ok("Ok");
        }

        [HttpDelete("DeleteStoryAndPhotos")]
        public async Task<ActionResult> DeleteStoryAndPhotos(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await GalleryService.GetStoryPhotosAsync(userId, id);
            if (result == null || result.Count() == 0) return NotFound("Photos not found");
            foreach (var photo in result)
            {
                await GalleryService.RemovePhotoAsync(userId, photo.Id);
            }
            await StoryService.RemoveStoryAsync(userId, id);
            return Ok("Ok");
        }
    }
}
