using System.Linq;
using System.Web.Mvc;
using Truffler;
using TrufflerSample.Models;

namespace TrufflerSample.Controllers
{
    public class RestaurantController : Controller
    {
        IClient client;

        public RestaurantController(IClient client)
        {
            this.client = client;
        }

        //
        // GET: /Restaurant/

        public ActionResult Index()
        {
            var restaurants = client.Search<Restaurant>().GetResult();
            return View(restaurants);
        }

        //
        // GET: /Restaurant/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Restaurant/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Restaurant/Create

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
        
        //
        // GET: /Restaurant/Edit/5

        public ActionResult Edit(string wikiurl)
        {
            return View(client.Get<Restaurant>(Server.UrlDecode(wikiurl)));
        }

        //
        // POST: /Restaurant/Edit/5

        [HttpPost]
        public ActionResult Edit(string wikiurl, FormCollection collection, Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                return View(restaurant);
            }

            client.Index(restaurant);

            return RedirectToAction("Index");
        }

        //
        // GET: /Restaurant/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Restaurant/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
