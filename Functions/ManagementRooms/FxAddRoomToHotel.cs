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
    public class FxAddRoomToHotel
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxAddRoomToHotel(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxAddRoomToHotel")]
        [OpenApiOperation(operationId: "AddRoomToHotel", tags: new[] { "Rooms" }, Summary = "Add a new room to a hotel", Description = "This endpoint adds a new room to a specific hotel.")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to add the room to.")]
        [OpenApiRequestBody("application/json", typeof(AddRoomRequestInput), Description = "Room details for adding", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hotels/{hotelId}/rooms")] HttpRequest req,
           ILogger log, Guid hotelId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var roomData = JsonConvert.DeserializeObject<AddRoomRequestInput>(requestBody);

                if (roomData == null || string.IsNullOrWhiteSpace(roomData.RoomType))
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'RoomType' is required.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var guidRoom = await _hotelManagementService.AddRoomToHotel(hotelId, roomData);

                return HttpResponseHelper.SuccessfulObjectResult(new ResponseResult
                {
                    IsError = false,
                    Message = $"Room added successfully: {guidRoom}",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while adding the room to the hotel.");
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
