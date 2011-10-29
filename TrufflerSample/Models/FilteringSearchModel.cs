using System.Collections.Generic;
using System.Web.Mvc;

namespace TrufflerSample.Models
{
    public class FilteringSearchModel
    {
        public FilteringSearchModel(IEnumerable<SelectListItem> cuisines, IEnumerable<SelectListItem> countries)
        {
            Cuisines = cuisines;
            Countries = countries;
        }

        public IEnumerable<SelectListItem> Cuisines { get; private set; }

        public IEnumerable<SelectListItem> Countries { get; private set; }

        public SearchResult SearchResult { get; set; }
    }
}
