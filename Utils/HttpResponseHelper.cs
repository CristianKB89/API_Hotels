using API_Hotels.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API_Hotels.Utils
{
    public class HttpResponseHelper
    {
        public static IActionResult SuccessfulObjectResult(object response)
        {
            return new OkObjectResult(response);
        }

        public static IActionResult BadRequestObjectResult(object response)
        {
            return new BadRequestObjectResult(response);
        }

        public static IActionResult HttpExplicitNoContent()
        {
            return new NoContentResult();
        }

        public static ObjectResult Response(int code, object value)
        {
            var result = new ObjectResult(value);
            result.StatusCode = code;
            return result;
        }

        public static IActionResult InternalServerErrorObjectResult(string errorMessage)
        {
            return InternalServerErrorObjectResult(new ResponseResult
            {
                IsError = true,
                Message = errorMessage
            });
        }

        public static IActionResult InternalServerErrorObjectResult(Exception ex)
        {
            return InternalServerErrorObjectResult(new ResponseResult
            {
                IsError = true,
                Message = ex.Message
            });
        }

        public static IActionResult InternalServerErrorObjectResult(ResponseResult response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
