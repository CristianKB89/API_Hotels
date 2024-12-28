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

namespace API_Hotels.Functions.ManagementHotels
{
    public class FxUpdateHotel
    {
        private readonly IHotelManagementService _hotelManagementServices;

        public FxUpdateHotel(IHotelManagementService hotelManagementServices)
        {
            _hotelManagementServices = hotelManagementServices;
        }

        [FunctionName("FxUpdateHotel")]
        [OpenApiOperation(operationId: "UpdateHotel", tags: new[] { "Hotels" }, Summary = "Update an existing hotel", Description = "This endpoint updates the details of an existing hotel.")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the hotel to update.")]
        [OpenApiRequestBody("application/json", typeof(HotelUpdateRequestInput), Description = "Hotel details for update", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The operation result.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "hotels/{hotelId}")] HttpRequest req, ILogger log, Guid hotelId)
        {
            log.LogInformation("Processing request to update hotel: {HotelId}", hotelId);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<HotelUpdateRequestInput>(requestBody);

                if (data == null || string.IsNullOrWhiteSpace(data.Name) || string.IsNullOrWhiteSpace(data.Location))
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'Name' and 'Location' are required.",
                        Timestamp = DateTime.UtcNow
                    });
                }
                var updatedHotel = new HotelUpdateRequestInput
                {
                    Name = data.Name,
                    Location = data.Location,
                    BasePrice = data.BasePrice,
                    Status = data.Status
                };

                await _hotelManagementServices.UpdateHotel(hotelId, updatedHotel);

                log.LogInformation("Hotel updated successfully: {HotelId}", hotelId);

                return HttpResponseHelper.SuccessfulObjectResult(updatedHotel);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while updating the hotel.");
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

