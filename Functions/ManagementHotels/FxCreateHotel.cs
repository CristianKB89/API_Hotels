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
    public class FxCreateHotel
    {
        private readonly ILogger<FxCreateHotel> _logger;
        private readonly IHotelManagementService _hotelManagementServices;

        public FxCreateHotel(ILogger<FxCreateHotel> log, IHotelManagementService hotelManagementServices)
        {
            _logger = log;
            _hotelManagementServices = hotelManagementServices;
        }

        [FunctionName("FxCreateHotel")]
        [OpenApiOperation(operationId: "CreateHotel", tags: new[] { "Hotels" }, Summary = "Create a new hotel", Description = "This endpoint creates a new hotel and saves it to the database.")]
        [OpenApiRequestBody("application/json", typeof(HotelCreateRequestInput), Description = "Hotel details for creation", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResponseResult), Description = "The created hotel details.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "CreateHotel")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing request to create a new hotel.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<HotelCreateRequestInput>(requestBody);

                if (data == null || string.IsNullOrWhiteSpace(data.Name) || string.IsNullOrWhiteSpace(data.Location))
                {
                    return new BadRequestObjectResult(new ResponseResult
                    {
                        IsError = true,
                        Message = "Invalid request body. 'Name' and 'Location' are required.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var newHotel = new HotelCreateRequestInput
                {
                    Name = data.Name,
                    Location = data.Location,
                    BasePrice = data.BasePrice
                };

                ///Insertar en base de datos
                var hotelID = await _hotelManagementServices.CreateHotel(newHotel);

                log.LogInformation($"Hotel created successfully: {hotelID}");

                return HttpResponseHelper.SuccessfulObjectResult(newHotel);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while creating the hotel.");
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

