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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace API_Hotels.Functions.ManagementReservations
{
    public class FxSearchHotels
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxSearchHotels(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxSearchHotels")]
        [OpenApiOperation(operationId: "SearchHotels", tags: new[] { "Reservations" }, Summary = "Search hotels based on filters", Description = "This endpoint retrieves hotels that match the given search criteria.")]
        [OpenApiParameter(name: "city", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The city where the hotel is located.")]
        [OpenApiParameter(name: "checkInDate", In = ParameterLocation.Query, Required = true, Type = typeof(DateTime), Description = "The check-in date.")]
        [OpenApiParameter(name: "checkOutDate", In = ParameterLocation.Query, Required = true, Type = typeof(DateTime), Description = "The check-out date.")]
        [OpenApiParameter(name: "numGuests", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The number of guests.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Hotels>), Description = "The list of matching hotels.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hotels/search")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing request to search hotels.");

            try
            {
                string city = req.Query["city"].FirstOrDefault();
                DateTime checkInDate = DateTime.Parse(req.Query["checkInDate"]);
                DateTime checkOutDate = DateTime.Parse(req.Query["checkOutDate"]);
                int numGuests = int.Parse(req.Query["numGuests"]);

                var hotels = await _hotelManagementService.SearchHotels(city, checkInDate, checkOutDate, numGuests);

                return HttpResponseHelper.SuccessfulObjectResult(hotels);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while searching for hotels.");
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
