namespace CertiFlowTeam.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string ErrorCode { get; set; }

        public static ServiceResult SuccessResult(string message = "İşlem başarılı", object data = null)
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult ErrorResult(string message, string errorCode = null)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string ErrorCode { get; set; }

        public static ServiceResult<T> SuccessResult(T data, string message = "İşlem başarılı")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> ErrorResult(string message, string errorCode = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
