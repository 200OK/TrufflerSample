using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class AutocompleteController : Controller
    {
        IClient client;

        public AutocompleteController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q)
        {
            if (q == null)
            {
                return View();
            }

            var results = client.Search<Restaurant>()
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

        public JsonResult Prefix(string term)
        {
            var results = client.Search<Restaurant>()
                .Filter(x => x.Name.PrefixCaseInsensitive(term))
                .Select(x => x.Name)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetResult();

            return Json(results.Select(x => x), JsonRequestBehavior.AllowGet);
        }
    }
}
