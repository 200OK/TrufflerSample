using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Truffler;
using Truffler.Api.Facets;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class FacetsController : Controller
    {
        IClient client;

        public FacetsController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index(string q, string cuisine, string country, int? rating)
        {
            //As UrlHelper.Action doesn't add query string parameter with empty values
            //we need to check all parameters in case the user has searched without
            //entering a search term (he/she has just hit the search button)
            if (q == null && cuisine == null && country == null && !rating.HasValue)
            {
                return View();
            }

            var query = client.Search<Restaurant>()
                .For(q)
                .TermsFacetFor(x => x.Cuisine)
                .TermsFacetFor(x => x.Country)
                .HistogramFacetFor(x => x.MichelinRating, 1);

            //Apply filters added by facet links
            if (!string.IsNullOrWhiteSpace(cuisine))
            {
                query = query.Filter(x => x.Cuisine.MatchCaseInsensitive(cuisine));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Filter(x => x.Country.MatchCaseInsensitive(country));
            }

            if (rating.HasValue)
            {
                query = query.Filter(x => x.MichelinRating.Match(rating.Value));
            }

            var results = query.Select(x => new SearchHit
                {
                    Title = !string.IsNullOrEmpty(x.Name.AsHighlighted()) ? x.Name.AsHighlighted() : x.Name,
                    Url = x.Website ?? x.WikipediaUrl,
                    Location = new List<string> { x.StreetAddress, x.City, x.Country }.Concatenate(", "),
                    MichelinRating = x.MichelinRating ?? 0
                })
                .GetResult();

            ViewBag.Query = q;
            ViewBag.Id = results.ProcessingInfo.ServerDuration;
            ViewBag.Hits = results.TotalMatching;
            
            //Groups of links for each facet
            var facets = new List<FacetResult>();
            
            var cuisineFacet = (TermsFacet) results.Facets["Cuisine"];
            var cuisineLinks = new FacetResult("Cuisines",
                cuisineFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Index", new { q, cuisine = x.Term, country, rating })
                }));
            facets.Add(cuisineLinks);

            var countryFacet = (TermsFacet)results.Facets["Country"];
            var countryLinks = new FacetResult("Countries",
                countryFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Index", new { q, cuisine, country = x.Term, rating })
                }));
            facets.Add(countryLinks);

            var ratingFacet = results.HistogramFacetFor(x => x.MichelinRating);
            var ratingFacetLinks = new FacetResult("Guide Michelin Rating",
                ratingFacet.Entries.Select(x => new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + x.Key,
                    Count = x.Count ,
                    Url = Url.Action("Index", new { q, cuisine, country, rating = x.Key })
                }));
            facets.Add(ratingFacetLinks);

            //Links for removing filters
            ViewBag.Filters = new List<FacetLink>();
            
            if (!string.IsNullOrEmpty(cuisine))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = cuisine,
                    Url = Url.Action("Index", new { q, country, rating })
                });
            }

            if (!string.IsNullOrEmpty(country))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = country,
                    Url = Url.Action("Index", new { q, cuisine, rating })
                });
            }

            if (rating.HasValue)
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + rating,
                    Url = Url.Action("Index", new { q, country, cuisine })
                });
            }

            return View(new SearchResult(results, q) { Facets = facets });
        }
    }
}
