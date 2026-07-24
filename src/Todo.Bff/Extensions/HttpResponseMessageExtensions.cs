using System.Net.Mime;

namespace Todo.Bff.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        // this là để biết đúng thằng gọi, ngay chóc thằng đó để lấy đúng response của thằng đó để mà trả về.
        public static async Task<IResult> ToResultAsync(this HttpResponseMessage response) 
        {
            // Soooo thằng Content.ReadAsStringAsync() sẽ đọc và cho chuỗi, và NoContent nó cũng đọc là "". nên trả về luôn.
            // Thằng UI thì không cần trả về nhugnw nó cứ trả , nên trong trường hợp này thì NoContent thì lấy để xử lý riêng.
            // Hơi ngu nhưng idk 

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Results.NoContent();
            }

            var rawContent = await response.Content.ReadAsStringAsync();


            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                // Lấy đúng cái Content-Type của API gốc (thường là application/json)
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

                // Trả về Y CHANG nội dung và Status Code gốc
                return Results.Content(rawContent, contentType, statusCode: (int)response.StatusCode);
            }

            return Results.StatusCode((int)response.StatusCode);

            //Cách lỏ cũ
            //var rawContent = response.Content.ReadAsStringAsync();

            //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);

        }
    }
}
