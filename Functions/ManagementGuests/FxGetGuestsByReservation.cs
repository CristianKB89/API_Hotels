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

namespace API_Hotels.Functions.ManagementGuests
{
    public class FxGetGuestsByReservation
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxGetGuestsByReservation(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxGetGuestsByReservation")]
        [OpenApiOperation(operationId: "GetGuestsByReservation", tags: new[] { "Reservations" }, Summary = "Retrieve guests by reservation", Description = "This endpoint retrieves all guests associated with a specific reservation.")]
        [OpenApiParameter(name: "reservationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the reservation to retrieve guests for.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Guests>), Description = "The list of guests.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reservations/{reservationId}/guests")] HttpRequest req, ILogger log, Guid reservationId)
        {
            log.LogInformation("Processing request to retrieve guests for reservation: {ReservationId}", reservationId);

            try
            {
                var guests = await _hotelManagementService.GetGuestsByReservation(reservationId);

                return HttpResponseHelper.SuccessfulObjectResult(guests);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving guests for the reservation.");
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
