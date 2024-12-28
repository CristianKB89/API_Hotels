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
using System.Net;
using System.Threading.Tasks;


namespace API_Hotels.Functions.ManagementReservations
{
    public class FxGetReservationsByHotel
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxGetReservationsByHotel(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxGetReservationsByHotel")]
        [OpenApiOperation(operationId: "GetReservationsByHotel", tags: new[] { "Reservations" }, Summary = "Retrieve reservations by hotel", Description = "This endpoint retrieves all reservations associated with a specific hotel.")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to retrieve reservations for.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Reservations>), Description = "The list of reservations.")]

        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hotels/{hotelId}/reservations")] HttpRequest req, ILogger log, Guid hotelId)
        {
            log.LogInformation("Processing request to retrieve reservations for hotel: {HotelId}", hotelId);

            try
            {
                var reservations = await _hotelManagementService.GetReservationsByHotel(hotelId);

                return HttpResponseHelper.SuccessfulObjectResult(reservations);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving reservations for the hotel.");
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
