using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using Microsoft.AspNetCore.Authorization;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _trackingService;
        private readonly ILogger<TrackingController> _logger;

        public TrackingController(
            ITrackingService trackingService,
            ILogger<TrackingController> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tracking theo userId
        /// </summary>
        [HttpGet("getbyuserid/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var trackings = await _trackingService.GetTrackingsByUserIdAsync(userId);
                return Ok(new { success = true, data = trackings, message = "Lấy danh sách tracking thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get tracking by user id endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Tạo tracking mới
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateTracking([FromBody] CreateTrackingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _trackingService.CreateTrackingAsync(request, HttpContext);
                
                if (!result)
                {
                    return BadRequest(new { message = "Tạo tracking thất bại" });
                }

                return Ok(new { message = "Tạo tracking thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in create tracking endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}

