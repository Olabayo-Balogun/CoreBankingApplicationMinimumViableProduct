namespace Application.Models
{
    public class RequestResponse<T>
    {
        private RequestResponse (bool isSuccessful, int statusCode, string remark, long totalCount, T? data)
        {
            IsSuccessful = isSuccessful;
            StatusCode = statusCode;
            Remark = remark;
            TotalCount = totalCount;
            Data = data;
        }

        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
        public string Remark { get; set; }
        public long TotalCount { get; set; }
        public T? Data { get; set; }

        public static RequestResponse<T> Success (T data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, remark, totalCount, data);
        }

        public static RequestResponse<T> SearchSuccessful (T data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, $"{remark} retrieved successfully", totalCount, data);
        }

        public static RequestResponse<T> NotFound (T? data, string remark)
        {
            return new RequestResponse<T> (false, 404, $"{remark} not found", 0, data);
        }

        public static RequestResponse<T> AlreadyExists (T? data, long totalCount, string remark)
        {
            return new RequestResponse<T> (false, 400, $"{remark} already exists", totalCount, data);
        }

        public static RequestResponse<T> CountSuccessful (T? data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, $"{remark} count successful", totalCount, data);
        }

        public static RequestResponse<T> Deleted (T? data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, $"{remark} deleted sucessfully", totalCount, data);
        }

        public static RequestResponse<T> Created (T data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 201, $"{remark} creation successful", totalCount, data);
        }

        public static RequestResponse<T> Updated (T data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, $"{remark} update successful", totalCount, data);
        }

        public static RequestResponse<T> Approved (T data, long totalCount, string remark)
        {
            return new RequestResponse<T> (true, 200, $"{remark} approval successful", totalCount, data);
        }

        public static RequestResponse<T> Error (T? data)
        {
            return new RequestResponse<T> (false, 500, "An error occurred", 0, data);
        }

        public static RequestResponse<T> Failed (T? data, int statusCode, string remark)
        {
            return new RequestResponse<T> (false, statusCode, remark, 0, data);
        }

        public static RequestResponse<T> NullPayload (T? data)
        {
            return new RequestResponse<T> (false, 400, "Please enter valid details", 0, data);
        }

        public static RequestResponse<T> AuditLogFailed (T? data)
        {
            return new RequestResponse<T> (false, 500, "Update failed please try again later", 0, data);
        }

        public static RequestResponse<T> Unauthorized (T? data, string remark)
        {
            return new RequestResponse<T> (false, 401, remark, 0, data);
        }
    }
}
