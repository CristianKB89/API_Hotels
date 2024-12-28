using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System;

namespace API_Hotels.Models
{
    public class ResponseResult
    {
        [JsonProperty("isError")]
        [OpenApiProperty(Description = "Bandera que indica si se produjo un error al momento de generar la solicitud", Nullable = false)]
        public bool IsError { get; set; }

        [JsonProperty("message")]
        [OpenApiProperty(Description = "Mensaje que indica el detalle del error", Nullable = false)]
        public string Message { get; set; }

        [JsonProperty("timeStamp")]
        [OpenApiProperty(Description = "Fecha en la cual se produjo el error", Default = "DateTime.UtcNow", Nullable = false)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
