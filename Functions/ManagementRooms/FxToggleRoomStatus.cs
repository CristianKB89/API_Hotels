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

namespace API_Hotels.Functions.ManagementRooms
{
    public class FxToggleRoomStatus
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxToggleRoomStatus(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxToggleRoomStatus")]
        [OpenApiOperation(operationId: "ToggleRoomStatus", tags: new[] { "Rooms" }, Summary = "Toggle room status", Description = "This endpoint toggles the status of a specific room in a hotel (enabled/disabled).")]
        [OpenApiParameter(name: "roomId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the room to toggle.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "rooms/{roomId}/toggle-status")] HttpRequest req,
            ILogger log, Guid roomId)
        {
            log.LogInformation("Processing request to toggle room status: {RoomId}", roomId);

            try
            {
                bool newStatus = await _hotelManagementService.ToggleRoomStatus(roomId);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = $"Room status updated successfully. New status: {(newStatus ? "Enabled" : "Disabled")}",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while toggling the room status.");
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
