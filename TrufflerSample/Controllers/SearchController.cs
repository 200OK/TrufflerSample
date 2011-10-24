using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Truffler;
using Truffler.Api.Facets;
using Truffler.Helpers.Text;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class SearchController : Controller
    {
        IClient client;

        public SearchController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Basic(string q)
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

        public ActionResult Paging(string q, int? p)
        {
            if (q == null)
            {
                return View();
            }

            int pageSize = 10;
            var page = p ?? 1;

            var results = client.Search<Restaurant>(Language.English)
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

        public ActionResult Highlighting(string q)
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

        public ActionResult Facets(string q, string cuisine, string country, int? rating)
        {
            if (q == null)
            {
                return View();
            }

            var query = client.Search<Restaurant>(Language.English)
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
            
            //Groups of links for each facet
            var facets = new List<FacetResult>();
            
            var cuisineFacet = (TermsFacet) results.Facets["Cuisine"];
            var cuisineLinks = new FacetResult("Cuisines",
                cuisineFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Facets", new { q, cuisine = x.Term, country, rating })
                }));
            facets.Add(cuisineLinks);

            var countryFacet = (TermsFacet)results.Facets["Country"];
            var countryLinks = new FacetResult("Countries",
                countryFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Facets", new { q, cuisine, country = x.Term, rating })
                }));
            facets.Add(countryLinks);

            var ratingFacet = results.HistogramFacetFor(x => x.MichelinRating);
            var ratingFacetLinks = new FacetResult("Guide Michelin Rating",
                ratingFacet.Entries.Select(x => new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + x.Key,
                    Count = x.Value,
                    Url = Url.Action("Facets", new { q, cuisine, country, rating = x.Key })
                }));
            facets.Add(ratingFacetLinks);

            //Links for removing filters
            ViewBag.Filters = new List<FacetLink>();
            
            if (!string.IsNullOrEmpty(cuisine))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = cuisine,
                    Url = Url.Action("Facets", new { q, country, rating })
                });
            }

            if (!string.IsNullOrEmpty(country))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = country,
                    Url = Url.Action("Facets", new { q, cuisine, rating })
                });
            }

            if (rating.HasValue)
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + rating,
                    Url = Url.Action("Facets", new { q, country, cuisine })
                });
            }

            return View(new SearchResult(results, q) { Facets = facets });
        }
    }
}
