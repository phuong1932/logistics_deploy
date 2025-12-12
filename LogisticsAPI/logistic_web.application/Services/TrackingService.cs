using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;
using Microsoft.AspNetCore.Http;

namespace logistic_web.application.Services
{
    public interface ITrackingService
    {
        Task<IEnumerable<TrackingResponse>> GetTrackingsByUserIdAsync(int userId);
        Task<bool> CreateTrackingAsync(CreateTrackingRequest request, HttpContext httpContext);
    }

    public class TrackingService : ITrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(
            IUnitOfWork unitOfWork,
            ILogger<TrackingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TrackingResponse>> GetTrackingsByUserIdAsync(int userId)
        {
            try
            {
                var trackings = await _unitOfWork.TrackingRepository.GetTrackingsByUserIdAsync(userId);
                
                return trackings.Select(t => new TrackingResponse
                {
                    Id = t.Id,
                    Username = t.Username,
                    Action = t.Action,
                    DateCreated = t.DateCreated,
                    Ip = t.Ip
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trackings for user: {UserId}", userId);
                return new List<TrackingResponse>();
            }
        }

        public async Task<bool> CreateTrackingAsync(CreateTrackingRequest request, HttpContext httpContext)
        {
            try
            {
               

                // Lấy IP address từ HttpContext
                var ipAddress = GetIpAddress(httpContext);

                // Tạo tracking mới
                var tracking = new Tracking
                {
                    Username = request.Username,
                    Action = request.Action,
                    DateCreated = DateTime.Now,
                    Ip = ipAddress
                };

                await _unitOfWork.TrackingRepository.AddAsync(tracking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Tracking created successfully for user: {Username}, Action: {Action}", 
                    request.Username, request.Action);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tracking for user: {Username}", request.Username);
                return false;
            }
        }

        private string GetIpAddress(HttpContext httpContext)
        {
            // Lấy IP address từ request
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // Kiểm tra X-Forwarded-For header (nếu có proxy/load balancer)
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            // Kiểm tra X-Real-IP header
            else if (httpContext.Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = httpContext.Request.Headers["X-Real-IP"].ToString();
            }

            return ipAddress ?? "Unknown";
        }
    }
}

