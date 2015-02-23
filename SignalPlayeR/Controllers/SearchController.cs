using RestSharp;
using System.Configuration;
using System.Web.Mvc;

namespace SignalPlayeR.Controllers
{
    public class SearchController : Controller
    {
        private string tinySongApiKey = ConfigurationManager.AppSettings["TinySongApiKey"];
        private string echoNestApiKey = ConfigurationManager.AppSettings["EchoNestApiKey"];

        // GET: /Search/
        public ActionResult Songs(string id)
        {
            var requestPath = "s/{song}?format=json&limit=5&key=" + tinySongApiKey;
            var client = new RestClient("http://tinysong.com");
            var request = new RestRequest(requestPath, Method.GET);
            request.AddParameter("song", id, ParameterType.UrlSegment);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            ViewBag.result = content;
            Response.ContentType = "application/json";
            return PartialView();
        }

        // GET: /Search/Duration/{title}/{artist}
        public ActionResult Duration(string title, string artist)
        {
            var requestPath = "api/v4/song/search?format=json&results=1&bucket=audio_summary&title={title}&artist={artist}&api_key=" + echoNestApiKey;
            var client = new RestClient("http://developer.echonest.com");
            var request = new RestRequest(requestPath, Method.GET);
            request.AddParameter("title", title, ParameterType.UrlSegment);
            request.AddParameter("artist", artist, ParameterType.UrlSegment);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            ViewBag.result = content;
            Response.ContentType = "application/json";
            return PartialView();
        }

    }
}