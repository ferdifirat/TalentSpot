namespace TalentSpot.Application.DTOs
{
    public class ResponseMessage<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }  
        public string Message { get; set; } 

        public static ResponseMessage<T> SuccessResponse(T data, string message = null)
        {
            return new ResponseMessage<T>
            {
                Data = data,
                Success = true,
                Message = message
            };
        }

        public static ResponseMessage<T> FailureResponse(string errorMessage)
        {
            return new ResponseMessage<T>
            {
                Success = false,
                Message = errorMessage,
            };
        }
    }
}
