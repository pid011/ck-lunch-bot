using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CKLunchBot.Core.Requester
{
    public interface IRequester
    {
        Task<JObject> RequestData();
    }
}
