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
namespace API_Hotels.Functions.ManagementReservations
{
    public class FxGetReservationDetails
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxGetReservationDetails(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxGetReservationDetails")]
        [OpenApiOperation(operationId: "GetReservationDetails", tags: new[] { "Reservations" }, Summary = "Retrieve reservation details", Description = "This endpoint retrieves the details of a specific reservation by its ID.")]
        [OpenApiParameter(name: "reservationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the reservation to retrieve.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Reservations), Description = "The details of the reservation.")]

        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "reservations/{reservationId}")] HttpRequest req,
            ILogger log, Guid reservationId)
        {
            log.LogInformation("Processing request to retrieve reservation details: {ReservationId}", reservationId);

            try
            {
                var reservation = await _hotelManagementService.GetReservationDetails(reservationId);

                if (reservation == null)
                {
                    return new NotFoundObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Reservation not found.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return HttpResponseHelper.SuccessfulObjectResult(reservation);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving the reservation details.");
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
