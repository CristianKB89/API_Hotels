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

namespace API_Hotels.Functions.ManagementRooms
{
    public class FxUpdateRoom
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxUpdateRoom(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxUpdateRoom")]
        [OpenApiOperation(operationId: "UpdateRoom", tags: new[] { "Rooms" }, Summary = "Update room details", Description = "This endpoint updates the details of a specific room in a hotel.")]
        [OpenApiParameter(name: "roomId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the room to update.")]
        [OpenApiRequestBody("application/json", typeof(UpdateRoomRequestInput), Description = "Updated room details", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "rooms/{roomId}")] HttpRequest req, ILogger log, Guid roomId)
        {
            log.LogInformation("Processing request to update room: {RoomId}", roomId);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var roomData = JsonConvert.DeserializeObject<UpdateRoomRequestInput>(requestBody);

                if (roomData == null || string.IsNullOrWhiteSpace(roomData.RoomType))
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'RoomType' is required.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                await _hotelManagementService.UpdateRoom(roomId, roomData);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = "Room updated successfully.",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while updating the room.");
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
