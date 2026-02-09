using ResponseLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Base.ResponseBase
{
    public abstract class ResponseBase : IResponseBase
    {
        public ErrorResponse GetErrorRequest(string error, string description)
        {
            return new ErrorResponse
            {
                Error = error,
                ErrorDescription = description
            };
        }
    }
}
