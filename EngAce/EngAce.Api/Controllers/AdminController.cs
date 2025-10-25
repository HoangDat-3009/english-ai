using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IWebHostEnvironment env, ILogger<AdminController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Upload one or more files. Accepts multipart/form-data with field name 'files'.
        /// Optional form field 'testType' groups files under that folder.
        /// NOTE: This endpoint currently allows anonymous access for dev; add [Authorize] when auth is ready.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile[] files, [FromForm] string? testType)
        {
            if (files == null || files.Length == 0)
            {
                return BadRequest(new { message = "No files uploaded" });
            }

            // Determine web root (fall back to ./wwwroot)
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var group = string.IsNullOrWhiteSpace(testType) ? "general" : SanitizeFolderName(testType!);
            var uploadsRoot = Path.Combine(webRoot, "uploads", group);
            Directory.CreateDirectory(uploadsRoot);

            var results = new List<object>();

            foreach (var file in files)
            {
                try
                {
                    var ext = Path.GetExtension(file.FileName);
                    var storedName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsRoot, storedName);

                    await using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _logger.LogInformation("Uploaded file {Original} -> {Stored}", file.FileName, storedName);

                    results.Add(new
                    {
                        originalName = file.FileName,
                        storedName,
                        size = file.Length,
                        mimeType = file.ContentType,
                        relativeUrl = $"/uploads/{group}/{storedName}"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save uploaded file {FileName}", file.FileName);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save file", file = file.FileName });
                }
            }

            return Ok(new { files = results });
        }

        private static string SanitizeFolderName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) input = input.Replace(c, '_');
            return input.Replace(' ', '_');
        }
    }
}
