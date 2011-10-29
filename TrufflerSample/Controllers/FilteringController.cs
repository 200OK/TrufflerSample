using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Truffler;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class FilteringController : Controller
    {
        IClient client;

        public FilteringController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q, IEnumerable<string> cuisines, IEnumerable<string> countries)
        {
            //Retrieving values for fields that should be possible to filter on.
            //Often we would get these from a database, CMS or other source.
            var filterValues = client.Search<Restaurant>()
                .Take(0)
                .TermsFacetFor(x => x.Cuisine)
                .TermsFacetFor(x => x.Country)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetResult();

            ViewBag.Cuisines = filterValues.TermsFacetFor(x => x.Cuisine)
                .Select(x => new SelectListItem { Text = x.Term, Value = x.Term});

            ViewBag.Countries = filterValues.TermsFacetFor(x => x.Country)
                .Select(x => new SelectListItem { Text = x.Term, Value = x.Term});

            //As UrlHelper.Action doesn't add query string parameter with empty values
            //we need to check all parameters in case the user has searched without
            //entering a search term (he/she has just hit the search button)
            if (q == null && cuisines == null && countries == null)
            {
                return View();
            }

            ITypeSearch<Restaurant> query = client.Search<Restaurant>()
                .For(q);

            if (cuisines != null)
            {
                var cuisineFilter = client.BuildFilter<Restaurant>();
                foreach (var cuisine in cuisines)
                {
                    cuisineFilter = cuisineFilter.Or(x => x.Cuisine.Match(cuisine));
                }
                query = query.Filter(cuisineFilter);
            }

            if (countries != null)
            {
                var countryFilter = client.BuildFilter<Restaurant>();
                foreach (var country in countries)
                {
                    countryFilter = countryFilter.Or(x => x.Country.Match(country));
                }
                query = query.Filter(countryFilter);
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

            return View(new SearchResult(results, q));
        }
    }
}
