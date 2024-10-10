namespace TalentSpot.Application.DTOs
{
    public class ResponseMessage<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }  
        public string Message { get; set; } 

        public static ResponseMessage<T> SuccessResponse(T result, string message = null)
        {
            return new ResponseMessage<T>
            {
                Result = result,
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
