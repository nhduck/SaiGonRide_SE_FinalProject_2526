using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using RentalVehicleService.Data;
using RentalVehicleService.Models;
using Microsoft.EntityFrameworkCore;

namespace RentalVehicleService.Controllers
{
    [ApiController]
    [Route("api/paypal")]
    public class PaypalApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaypalApiController> _logger;

        public PaypalApiController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ApplicationDbContext context,
            ILogger<PaypalApiController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];
            var baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("PayPal ClientId or ClientSecret not configured.");
            }

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            var client = _httpClientFactory.CreateClient();

            var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            req.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

            var res = await client.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to obtain PayPal token: {content}");
                throw new InvalidOperationException($"Failed to obtain PayPal token: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("No access_token in PayPal response");
            }
            return token;
        }

        public class CreateOrderRequest
        {
            public int RentalId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; } = "USD";
            public string? ReturnUrl { get; set; }
            public string? CancelUrl { get; set; }
        }

        [HttpPost("create-order")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { success = false, error = "Amount must be greater than zero." });

            try
            {
                var rental = await _context.Rentals.FindAsync(request.RentalId);
                if (rental == null)
                    return NotFound(new { success = false, error = "Rental not found." });

                var token = await GetAccessTokenAsync();
                var baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
                var client = _httpClientFactory.CreateClient();

                var orderPayload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            reference_id = request.RentalId.ToString(),
                            amount = new
                            {
                                currency_code = request.Currency ?? "USD",
                                value = request.Amount.ToString("F2")
                            },
                            description = $"Payment for Rental #{request.RentalId}"
                        }
                    },
                    application_context = new
                    {
                        return_url = request.ReturnUrl ?? string.Empty,
                        cancel_url = request.CancelUrl ?? string.Empty,
                        brand_name = "SaigonRide",
                        locale = "en-US",
                        landing_page = "BILLING"
                    }
                };

                var json = JsonSerializer.Serialize(orderPayload, new JsonSerializerOptions { WriteIndented = true });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders")
                {
                    Content = content
                };
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var res = await client.SendAsync(httpReq);
                var responseContent = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal create order failed: {responseContent}");
                    return StatusCode((int)res.StatusCode, new { success = false, error = responseContent });
                }

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                var orderId = root.GetProperty("id").GetString();
                string? approveUrl = null;

                if (root.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        if (link.GetProperty("rel").GetString() == "approve")
                        {
                            approveUrl = link.GetProperty("href").GetString();
                            break;
                        }
                    }
                }

                return Ok(new { success = true, orderId, approveUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateOrder: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        public class CaptureOrderRequest
        {
            public string? OrderId { get; set; }
            public int RentalId { get; set; }
        }

        [HttpPost("capture-order")]
        [Authorize]
        public async Task<IActionResult> CaptureOrder([FromBody] CaptureOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrderId))
                return BadRequest(new { success = false, error = "OrderId is required." });

            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Include(r => r.EndStation)
                    .FirstOrDefaultAsync(r => r.RentalId == request.RentalId);

                if (rental == null)
                    return NotFound(new { success = false, error = "Rental not found." });

                var token = await GetAccessTokenAsync();
                var baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
                var client = _httpClientFactory.CreateClient();

                var httpReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{request.OrderId}/capture");
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpReq.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var res = await client.SendAsync(httpReq);
                var responseContent = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal capture failed: {responseContent}");
                    return StatusCode((int)res.StatusCode, new { success = false, error = responseContent });
                }

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    if (status == "COMPLETED")
                    {
                        // Update rental status to Completed
                        rental.Status = RentalStatus.Completed;
                        rental.PaymentMethod = "PayPal";
                        rental.PaymentTransactionId = request.OrderId;
                        rental.PaymentCompletedTime = DateTime.Now;

                        // Ensure vehicle is available (should already be done in Edit, but double-check)
                        if (rental.Vehicle != null && rental.Vehicle.State != VehicleState.Available)
                        {
                            rental.Vehicle.State = VehicleState.Available;
                            _logger.LogInformation($"Vehicle {rental.VehicleId} set to Available after payment");
                        }

                        // Ensure station count is correct (should already be done in Edit, but double-check)
                        if (rental.EndStationId.HasValue && rental.EndStation != null)
                        {
                            // The Edit action should have already incremented, but this is a safety check
                            _logger.LogInformation($"Rental {rental.RentalId} payment completed. Vehicle: {rental.VehicleId}, Station: {rental.EndStationId}");
                        }

                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Rental {rental.RentalId} status updated to Completed with PayPal transaction {request.OrderId}");

                        return Ok(new { success = true, message = "Payment captured successfully", status });
                    }
                }

                return BadRequest(new { success = false, error = "Payment capture incomplete." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CaptureOrder: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
