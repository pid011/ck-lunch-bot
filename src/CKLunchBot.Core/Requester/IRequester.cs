using Newtonsoft.Json.Linq;

using System.Threading.Tasks;

namespace CKLunchBot.Core.Requester
{
    public interface IRequester
    {
        Task<JObject> RequestData();
    }
}