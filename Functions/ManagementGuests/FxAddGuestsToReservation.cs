using API_Hotels.Models;
using API_Hotels.Models.Inputs;
using API_Hotels.Repositories.Interfaces;
using API_Hotels.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace API_Hotels.Functions.ManagementGuests
{
    public class FxAddGuestsToReservation
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxAddGuestsToReservation(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxAddGuestsToReservation")]
        [OpenApiOperation(operationId: "AddGuestsToReservation", tags: new[] { "Reservations" }, Summary = "Add guests to a reservation", Description = "This endpoint adds additional guests to an existing reservation.")]
        [OpenApiParameter(name: "reservationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the reservation to update.")]
        [OpenApiRequestBody("application/json", typeof(AddGuestsRequestInput), Description = "Details of the additional guests", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "reservations/{reservationId}/add-guests")] HttpRequest req, ILogger log, Guid reservationId)
        {
            log.LogInformation("Processing request to add guests to reservation: {ReservationId}", reservationId);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var addGuestsRequest = JsonConvert.DeserializeObject<AddGuestsRequestInput>(requestBody);

                if (addGuestsRequest == null || addGuestsRequest.NumberOfAdditionalGuests <= 0)
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'NumberOfAdditionalGuests' must be greater than zero.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                await _hotelManagementService.AddGuestsToReservation(reservationId, addGuestsRequest.NumberOfAdditionalGuests);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = "Guests added to reservation successfully.",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while adding guests to the reservation.");
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
