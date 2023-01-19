using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public static bool IsValid (string responseText, out string errorMessage)
        {
            var responseResult = JsonConvert.DeserializeObject<ApiResponse>(responseText);
            errorMessage = responseResult.Message;
            return responseResult.StatusCode == 200;
        }
    }
}
