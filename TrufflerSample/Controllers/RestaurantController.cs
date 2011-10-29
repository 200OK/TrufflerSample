using System.Linq;
using System.Web.Mvc;
using Truffler;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    //This controller illustrates how restaurants could be added and updated.
    //However, it won't work as the URL in web.config is read-only.
    public class RestaurantController : Controller
    {
        IClient client;

        public RestaurantController(IClient client)
        {
            this.client = client;
        }

        public ActionResult Index()
        {
            var restaurants = client.Search<Restaurant>().Take(100).GetResult();
            return View(restaurants);
        }

        public ActionResult Create()
        {
            return View();
        } 

        [HttpPost]
        public ActionResult Create(FormCollection collection, Restaurant newRestaurant)
        {
            if (!ModelState.IsValid)
            {
                return View(newRestaurant);
            }

            client.Index(newRestaurant);

            return RedirectToAction("Index");
        }
        
        public ActionResult Edit(string wikiurl)
        {
            return View(client.Get<Restaurant>(Server.UrlDecode(wikiurl)));
        }

        [HttpPost]
        public ActionResult Edit(string wikiurl, FormCollection collection, Restaurant restaurant, string tags)
        {
            if (!ModelState.IsValid)
            {
                return View(restaurant);
            }
            if (!string.IsNullOrWhiteSpace(tags))
            {
                restaurant.Tags = tags.Split(',').Select(x => x.Trim()).ToList();
            }
            client.Index(restaurant);

            return RedirectToAction("Index");
        }
    }
}
