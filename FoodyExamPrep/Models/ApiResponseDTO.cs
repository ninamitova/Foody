using System.Text.Json.Serialization;

namespace ApiResponseDTO.Models
{

    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; } // nullable because some responses may not include it
    }

}