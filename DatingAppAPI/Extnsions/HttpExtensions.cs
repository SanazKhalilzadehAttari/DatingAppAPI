using DatingAppAPI.Helpers;
using System.Text.Json;

namespace DatingAppAPI.Extnsions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse httpResponse,
            PaginationHeader paginationHeader) {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            httpResponse.Headers.Add("Pagination",JsonSerializer.Serialize(paginationHeader, jsonOptions));
            httpResponse.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
