using ResponseLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Base.ResponseBase
{
    public interface IResponseBase
    {
        ErrorResponse GetErrorRequest(string error, string description);
    }
}
