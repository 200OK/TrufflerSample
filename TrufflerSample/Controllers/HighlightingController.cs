using System.Collections.Generic;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class HighlightingController : Controller
    {
        IClient client;

        public HighlightingController(IClient client)
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
                    Title = !string.IsNullOrEmpty(x.Name.AsHighlighted()) ? x.Name.AsHighlighted() : x.Name,
                    Url = x.Website ?? x.WikipediaUrl,
                    Location = new List<string> { x.StreetAddress, x.City, x.Country }.Concatenate(", "),
                    MichelinRating = x.MichelinRating ?? 0,
                    Text = x.WikipediaText.AsHighlighted(
                        new HighlightSpec
                        {
                            FragmentSize = 200, 
                            NumberOfFragments = 2,
                            Concatenation = highlights => highlights.Concatenate(" ... ")
                        })
                })
                .GetResult();

            ViewBag.Query = q;

            return View(new SearchResult(results, q));
        }
    }
}
