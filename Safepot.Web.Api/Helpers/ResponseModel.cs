using System.Collections.Generic;
namespace Safepot.Web.Api.Helpers
{
    public class ResponseModel<T>
    {

        public string? Result { get; set; }
        public string? Message { get; set; }
        public IEnumerable<T>? Data { get; set; }
        public static ResponseModel<T> ToApiResponse(string result="Success",string message="",IEnumerable<T>? data=null)
        {
            ResponseModel<T> model = new ResponseModel<T>();
            model.Result = result;
            model.Message = message;
            model.Data = data;
            return model;
        }

    }
}
