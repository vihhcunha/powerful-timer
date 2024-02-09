namespace PowerfulTimer.Api.Models;

public class ResponseModel<T>
{
    public bool Success { get; set; }
    public T Object { get; set; }
}

public class ErrorResponseModel
{
    public string Message { get; set; }
}

public class ResponseModelBuilder
{
    public static ResponseModel<object> CreateSuccessResponse()
    {
        return new ResponseModel<object>
        {
            Success = true,
            Object = null
        };
    }

    public static ResponseModel<T> CreateSuccessResponse<T>(T data)
    {
        return new ResponseModel<T>
        {
            Success = true,
            Object = data
        };
    }

    public static ResponseModel<List<ErrorResponseModel>> CreateErrorResponse(List<ErrorResponseModel> errorsDetails)
    {
        return new ResponseModel<List<ErrorResponseModel>>
        {
            Success = false,
            Object = errorsDetails
        };
    }

    public static ResponseModel<List<ErrorResponseModel>> CreateErrorResponse(List<string> errorsDetails)
    {
        var errorResponseList = new List<ErrorResponseModel>();
        errorsDetails.ForEach(error => errorResponseList.Add(new ErrorResponseModel { Message = error }));
        return new ResponseModel<List<ErrorResponseModel>>
        {
            Success = false,
            Object = errorResponseList
        };
    }
}
