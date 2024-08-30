using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ToDo.Application.Common
{
    public class ResultModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
        public List<string> Errors { get; set; } = new List<string>();

        public static ResultModel<T> SuccessResult(T data , string message , List<string> errors)
        {
            return new ResultModel<T> { Success = true, Message = message , Data = data , Errors = errors };
        }

        public static ResultModel<T> ErrorResult(T data, string message , List<string> errors)
        {
            return new ResultModel<T> { Success = false, Message = message, Data = data , Errors = errors };
        }

    }

    public class ResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        public static ResultModel SuccessResult(string message)
        {
            return new ResultModel{ Success = true, Message = message };
        }

        public static ResultModel ErrorResult(string message)
        {
            return new ResultModel { Success = false, Message = message };
        }

    }
}
