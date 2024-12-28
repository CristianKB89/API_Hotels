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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace API_Hotels.Functions.ManagementReservations
{
    public class FxCreateReservation
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxCreateReservation(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxCreateReservation")]
        [OpenApiOperation(operationId: "CreateReservation", tags: new[] { "Reservations" }, Summary = "Create a new reservation", Description = "This endpoint creates a new reservation for a hotel room.")]
        [OpenApiRequestBody("application/json", typeof(CreateReservationRequestInput), Description = "Reservation details", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "reservations")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing request to create a new reservation.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var reservationData = JsonConvert.DeserializeObject<CreateReservationRequestInput>(requestBody);

                if (reservationData == null || reservationData.HotelId == Guid.Empty || reservationData.RoomId == Guid.Empty || reservationData.CheckInDate >= reservationData.CheckOutDate)
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. Ensure all required fields are filled and dates are correct.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                await _hotelManagementService.CreateReservation(reservationData);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = "Reservation created successfully.",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while creating the reservation.");
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
