using Helper;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyManagementController : ControllerBase
    {
        private readonly GeminiKeyManager _keyManager;
        private readonly ILogger<KeyManagementController> _logger;

        public KeyManagementController(GeminiKeyManager keyManager, ILogger<KeyManagementController> logger)
        {
            _keyManager = keyManager;
            _logger = logger;
        }

        /// <summary>
        /// Lấy trạng thái của tất cả API keys
        /// </summary>
        [HttpGet("Status")]
        public ActionResult<List<KeyStatus>> GetKeysStatus()
        {
            try
            {
                var status = _keyManager.GetKeysStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy trạng thái keys");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Rotate sang key tiếp theo (manual)
        /// </summary>
        [HttpPost("Rotate")]
        public ActionResult<string> RotateKey([FromQuery] string reason = "Manual rotation")
        {
            try
            {
                var newKey = _keyManager.RotateToNextKey(reason);
                var status = _keyManager.GetKeysStatus().First(k => k.IsCurrent);
                
                return Ok(new
                {
                    Message = "Đã chuyển sang key mới",
                    CurrentKeyIndex = status.Index,
                    KeyPreview = status.KeyPreview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi rotate key");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reset một key cụ thể về trạng thái active
        /// </summary>
        [HttpPost("Reset/{index}")]
        public ActionResult ResetKey(int index)
        {
            try
            {
                _keyManager.ResetKey(index);
                return Ok($"Đã reset key #{index}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset key");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reset tất cả keys về trạng thái active
        /// </summary>
        [HttpPost("ResetAll")]
        public ActionResult ResetAllKeys()
        {
            try
            {
                _keyManager.ResetAllKeys();
                return Ok("Đã reset tất cả keys");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset tất cả keys");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy key hiện tại đang sử dụng (chỉ preview)
        /// </summary>
        [HttpGet("Current")]
        public ActionResult GetCurrentKey()
        {
            try
            {
                var status = _keyManager.GetKeysStatus().First(k => k.IsCurrent);
                return Ok(new
                {
                    Index = status.Index,
                    KeyPreview = status.KeyPreview,
                    IsActive = status.IsActive,
                    FailureCount = status.FailureCount,
                    LastUsed = status.LastUsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy current key");
                return BadRequest(ex.Message);
            }
        }
    }
}
