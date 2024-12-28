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
    public class FxAddEmergencyContact
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxAddEmergencyContact(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxAddEmergencyContact")]
        [OpenApiOperation(operationId: "AddEmergencyContact", tags: new[] { "Reservations" }, Summary = "Add an emergency contact to a reservation", Description = "This endpoint adds an emergency contact to an existing reservation.")]
        [OpenApiParameter(name: "reservationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the reservation to update.")]
        [OpenApiRequestBody("application/json", typeof(AddEmergencyContactRequestInput), Description = "Details of the emergency contact", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "reservations/{reservationId}/add-emergency-contact")] HttpRequest req, ILogger log, Guid reservationId)
        {
            log.LogInformation("Processing request to add emergency contact to reservation: {ReservationId}", reservationId);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var emergencyContactRequest = JsonConvert.DeserializeObject<AddEmergencyContactRequestInput>(requestBody);

                if (emergencyContactRequest == null || string.IsNullOrWhiteSpace(emergencyContactRequest.FullName) || string.IsNullOrWhiteSpace(emergencyContactRequest.Phone))
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'Name' and 'PhoneNumber' are required.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                await _hotelManagementService.AddEmergencyContact(reservationId, emergencyContactRequest);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = "Emergency contact added to reservation successfully.",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while adding the emergency contact to the reservation.");
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
