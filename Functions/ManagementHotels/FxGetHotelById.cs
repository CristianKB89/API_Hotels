using API_Hotels.Models;
using API_Hotels.Repositories.Interfaces;
using API_Hotels.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace API_Hotels.Functions.ManagementHotels
{
    public class FxGetHotelById
    {
        private readonly ILogger<FxGetHotelById> _logger;
        private readonly IHotelManagementService _hotelManagementServices;

        public FxGetHotelById(ILogger<FxGetHotelById> log, IHotelManagementService hotelManagementServices)
        {
            _logger = log;
            _hotelManagementServices = hotelManagementServices;
        }


        [FunctionName("FxGetHotelById")]
        [OpenApiOperation(operationId: "GetHotelById", tags: new[] { "Hotels" }, Summary = "Retrieve a hotel by its ID", Description = "This endpoint retrieves a specific hotel by its ID.")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to retrieve.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Hotels), Description = "The hotel details.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hotels/{hotelId}")] HttpRequest req, ILogger log, Guid hotelId)
        {
            log.LogInformation("Processing request to retrieve hotel with ID: {HotelId}", hotelId);

            try
            {
                var hotel = await _hotelManagementServices.GetHotelById(hotelId);

                if (hotel == null)
                {
                    return new NotFoundObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Hotel not found.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return HttpResponseHelper.SuccessfulObjectResult(hotel);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving the hotel.");
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
