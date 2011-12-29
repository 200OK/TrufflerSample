using System.Collections.Generic;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class PagingController : Controller
    {
        IClient client;

        public PagingController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q, int? p)
        {
            if (q == null && !p.HasValue)
            {
                return View();
            }

            int pageSize = 10;
            var page = p ?? 1;

            var results = client.Search<Restaurant>()
                .For(q)
                .Take(pageSize)
                .Skip((page-1) * 10)
                .Select(x => new SearchHit
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

            var totalPages = results.TotalMatching/pageSize;
            if (results.TotalMatching % pageSize > 0)
            {
                totalPages++;
            }

            return View(new SearchResult(results, q)
                            {
                                CurrentPage = page, 
                                TotalPages = totalPages
                            });
        }
    }
}
