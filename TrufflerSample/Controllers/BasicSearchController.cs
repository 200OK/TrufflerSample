using System.Collections.Generic;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class BasicSearchController : Controller
    {
        IClient client;

        public BasicSearchController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q)
        {
            if (q == null)
            {
                return View();
            }

            var results = client.Search<Restaurant>(Language.English)
                .For(q)
                .Select(x => new SearchHit
                            {
                                Title = x.Name,
                                Url = x.Website ?? x.WikipediaUrl,
                                Location = new List<string> { x.StreetAddress, x.City, x.Country }.Concatenate(", "),
                                MichelinRating = x.MichelinRating ?? 0
                            })
                .GetResult();

            ViewBag.Query = q;

            return View(new SearchResult(results, q));
        }
    }
}
