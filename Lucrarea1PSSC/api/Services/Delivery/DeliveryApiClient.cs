using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Lucrarea1PSSC.api.DTOs.Delivery;

namespace Lucrarea1PSSC.api.Services.Delivery
{
    /// <summary>
    /// Typed HttpClient for Delivery API
    /// Handles communication with the external delivery service
    /// </summary>
    public class DeliveryApiClient
    {
        private readonly HttpClient _httpClient;

        public DeliveryApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Schedule a delivery for an order
        /// </summary>
        public async Task<DeliveryResponse> ScheduleDeliveryAsync(DeliveryRequest request)
        {
            try
            {
                Console.WriteLine($"[DELIVERY API] Scheduling delivery for order {request.OrderNumber}...");
                Console.WriteLine($"[DELIVERY API] Delivery to: {request.CustomerName}, {request.DeliveryAddress}");
                
                var response = await _httpClient.PostAsJsonAsync("/api/delivery/schedule", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var deliveryResponse = await response.Content.ReadFromJsonAsync<DeliveryResponse>();
                    Console.WriteLine($"[DELIVERY API] ? Delivery scheduled successfully. Tracking: {deliveryResponse?.TrackingNumber}");
                    return deliveryResponse ?? new DeliveryResponse 
                    { 
                        Success = false, 
                        Message = "Empty response from delivery service" 
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DELIVERY API] ? Failed to schedule delivery. Status: {response.StatusCode}");
                    Console.WriteLine($"[DELIVERY API] Error: {errorContent}");
                    
                    return new DeliveryResponse
                    {
                        Success = false,
                        Message = $"Delivery service returned {response.StatusCode}: {errorContent}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DELIVERY API] ? HTTP Request failed: {ex.Message}");
                throw; // Let Polly handle retries
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELIVERY API] ? Unexpected error: {ex.Message}");
                return new DeliveryResponse
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get delivery status
        /// </summary>
        public async Task<DeliveryStatusUpdate?> GetDeliveryStatusAsync(string trackingNumber)
        {
            try
            {
                Console.WriteLine($"[DELIVERY API] Checking status for tracking: {trackingNumber}...");
                
                var response = await _httpClient.GetAsync($"/api/delivery/status/{trackingNumber}");
                
                if (response.IsSuccessStatusCode)
                {
                    var status = await response.Content.ReadFromJsonAsync<DeliveryStatusUpdate>();
                    Console.WriteLine($"[DELIVERY API] ? Status retrieved: {status?.Status}");
                    return status;
                }
                
                Console.WriteLine($"[DELIVERY API] ? Failed to get status. Status code: {response.StatusCode}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DELIVERY API] ? HTTP Request failed: {ex.Message}");
                throw; // Let Polly handle retries
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELIVERY API] ? Error getting status: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Cancel a scheduled delivery
        /// </summary>
        public async Task<bool> CancelDeliveryAsync(string deliveryId, string reason)
        {
            try
            {
                Console.WriteLine($"[DELIVERY API] Cancelling delivery {deliveryId}. Reason: {reason}");
                
                var request = new { DeliveryId = deliveryId, Reason = reason };
                var response = await _httpClient.PostAsJsonAsync("/api/delivery/cancel", request);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DELIVERY API] ? Delivery cancelled successfully");
                    return true;
                }
                
                Console.WriteLine($"[DELIVERY API] ? Failed to cancel delivery. Status: {response.StatusCode}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DELIVERY API] ? HTTP Request failed: {ex.Message}");
                throw; // Let Polly handle retries
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELIVERY API] ? Error cancelling delivery: {ex.Message}");
                return false;
            }
        }
    }
}
