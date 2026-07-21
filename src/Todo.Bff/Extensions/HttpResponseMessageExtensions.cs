namespace Todo.Bff.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        // this là để biết đúng thằng gọi, ngay chóc thằng đó để lấy đúng response của thằng đó để mà trả về.
        public static async Task<IResult> ToResultAsync(this HttpResponseMessage response) 
        {
            // Như bên kia nhưng rút gọn lại cho dễ
            var rawContent = await response.Content.ReadAsStringAsync();

            return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
        }
    }
}
