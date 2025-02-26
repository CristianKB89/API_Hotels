using API_Hotels.Models;
using API_Hotels.Repositories.Interfaces;
using API_Hotels.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace API_Hotels.Functions.ManagementHotels
{
    public class FxGetHotels
    {
        private readonly IHotelManagementService _hotelManagementServices;

        public FxGetHotels(IHotelManagementService hotelManagementServices)
        {
            _hotelManagementServices = hotelManagementServices;
        }


        [FunctionName("FxGetHotels")]
        [OpenApiOperation(operationId: "GetHotels", tags: new[] { "Hotels" }, Summary = "Retrieve all hotels", Description = "This endpoint retrieves a list of all hotels.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Hotels>), Description = "The list of hotels.")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hotels")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing request to retrieve all hotels.");

            try
            {
                var hotels = await _hotelManagementServices.GetHotels();

                if (hotels == null || hotels.Count == 0)
                {
                    log.LogInformation("No hotels found.");
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }

                log.LogInformation($"Retrieved {hotels.Count} hotels successfully.");
                return HttpResponseHelper.SuccessfulObjectResult(hotels);
            }
            catch (KeyNotFoundException ex)
            {
                log.LogWarning(ex, "No hotels found.");
                return new ObjectResult(new ResponseResult
                {
                    IsError = true,
                    Message = "No hotels found.",
                    Timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving the hotels.");
                return new ObjectResult(new ResponseResult
                {
                    IsError = true,
                    Message = "An unexpected error occurred.",
                    Timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

    }
}
