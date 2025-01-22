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

namespace API_Hotels.Functions.ManagementRooms
{
    public class FxGetRoomsByHotel
    {
        private readonly IHotelManagementService _hotelManagementService;

        public FxGetRoomsByHotel(IHotelManagementService hotelManagementService)
        {
            _hotelManagementService = hotelManagementService;
        }

        [FunctionName("FxGetRoomsByHotel")]
        [OpenApiOperation(operationId: "GetRoomsByHotel", tags: new[] { "Rooms" }, Summary = "Retrieve rooms by hotel", Description = "This endpoint retrieves all rooms associated with a specific hotel.")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to retrieve rooms for.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Rooms>), Description = "The list of rooms.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hotels/{hotelId}/rooms")] HttpRequest req, ILogger log, Guid hotelId)
        {
            log.LogInformation("Processing request to retrieve rooms for hotel: {HotelId}", hotelId);

            try
            {
                var rooms = await _hotelManagementService.GetRoomsByHotel(hotelId);

                return HttpResponseHelper.SuccessfulObjectResult(rooms);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while retrieving rooms for the hotel.");
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
