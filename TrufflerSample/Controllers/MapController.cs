using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class MapController : Controller
    {
        IClient client;

        public MapController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q, string vertexes)
        {
            return View();
        }

        public ActionResult Filter(string vertexes)
        {
            var points = ParseVertexes(vertexes);
            var results = client.Search<Restaurant>()
                .Filter(x => x.Coordinates.Within(points))
                .Select(x => new SearchHit
                {
                    Title = x.Name,
                    Url = x.Website ?? x.WikipediaUrl,
                    Location = new List<string> { x.StreetAddress, x.City, x.Country }.Concatenate(", "),
                    MichelinRating = x.MichelinRating ?? 0
                })
                .GetResult();

            return PartialView("SearchHits", new SearchResult(results, ""));
        }

        IEnumerable<GeoLocation> ParseVertexes(string vertexes)
        {
            var split = vertexes.Split(';');
            foreach (var vertexString in split)
            {
                var latitudeString = vertexString.Split(',')[0];
                var latitude = double.Parse(latitudeString, CultureInfo.CreateSpecificCulture("en-us"));
                var longitudeString = vertexString.Split(',')[1];
                var longitude = double.Parse(longitudeString, CultureInfo.CreateSpecificCulture("en-us"));
                yield return new GeoLocation(latitude, longitude);
            }
        }
    }
}
