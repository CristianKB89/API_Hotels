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
    public class FxToggleHotelStatus
    {

        private readonly IHotelManagementService _hotelManagementServices;

        public FxToggleHotelStatus(IHotelManagementService hotelManagementServices)
        {
            _hotelManagementServices = hotelManagementServices;
        }

        [FunctionName("FxToggleHotelStatus")]
        [OpenApiOperation(operationId: "ToggleHotelStatus", tags: new[] { "Hotels" }, Summary = "Toggle the status of a hotel", Description = "This endpoint toggles the status of an existing hotel (enabled/disabled).")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to toggle.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "hotels/{hotelId}/toggle-status")] HttpRequest req, ILogger log, Guid hotelId)
        {
            log.LogInformation("Processing request to toggle status of hotel: {HotelId}", hotelId);

            try
            {
                bool newStatus = await _hotelManagementServices.ToggleHotelStatus(hotelId);

                log.LogInformation("Hotel status toggled successfully: {HotelId}, NewStatus: {NewStatus}", hotelId, newStatus);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = $"Hotel status updated successfully. New status: {(newStatus ? "Enabled" : "Disabled")}",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while toggling the hotel status.");
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
