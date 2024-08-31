using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ToDo.Application.Common.ResponseModels
{
    public class ResultModel<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
       

        public static ResultModel<T> SuccessResult( string message , T data)
        {
            return new ResultModel<T> { Status = true, Message = message, Data = data};
        }

        public static ResultModel<T> ErrorResult(string message, T data  )
        {
            return new ResultModel<T> { Status = false, Message = message, Data = data };
        }

    }

    public class ResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
       

        public static ResultModel SuccessResult(string message)
        {
            return new ResultModel { Success = true, Message = message };
        }

        public static ResultModel ErrorResult(string message)
        {
            return new ResultModel { Success = false, Message = message };
        }

    }
}
