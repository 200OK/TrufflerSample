using System.Collections.Generic;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class SortingController : Controller
    {
        IClient client;

        public SortingController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q, string sort)
        {
            if (q == null && sort == null)
            {
                return View();
            }

            ViewBag.SortLinks = new List<Link>
                {
                    new Link
                        {
                            Text = "Relevance",
                            Url = Url.Action("Index", new {q, sort = "relevance"}),
                            CssClass = (sort == null || sort == "relevance") ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Name",
                            Url = Url.Action("Index", new {q, sort = "name"}),
                            CssClass = sort == "name" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Cuisine",
                            Url = Url.Action("Index", new {q, sort = "cuisine"}),
                            CssClass = sort == "cuisine" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Country",
                            Url = Url.Action("Index", new {q, sort = "country"}),
                            CssClass = sort == "country" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Guide Michelin Rating",
                            Url = Url.Action("Index", new {q, sort = "rating"}),
                            CssClass = sort == "rating" ? "active" : ""
                        }
                };

        ITypeSearch<Restaurant> query = client.Search<Restaurant>().For(q);

        switch (sort)
        {
            case "name":
                query = query.OrderBy(x => x.Name);
                break;
            case "cuisine":
                query = query.OrderBy(x => x.Cuisine);
                break;
            case "country":
                query = query.OrderBy(x => x.Country);
                break;
            case "rating":
                query = query.OrderByDescending(x => x.MichelinRating);
                break;
        }

        var results = query.Select(x => new SearchHit
                {
                    Title = x.Name,
                    Url = x.Website ?? x.WikipediaUrl,
                    Location = new List<string> { x.StreetAddress, x.City, x.Country }.Concatenate(", "),
                    MichelinRating = x.MichelinRating ?? 0
                })
            .GetResult();

            ViewBag.Query = q;
            ViewBag.Id = results.ProcessingInfo.ServerDuration;
            ViewBag.Hits = results.TotalMatching;

            return View(new SearchResult(results, q));
        }
    }
}
