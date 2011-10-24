using System.Globalization;
using System.Web.Mvc;
using Truffler;

namespace TrufflerSample.Models
{
    public class GeoLocationModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var latitude = double.Parse(bindingContext.ValueProvider.GetValue("Coordinates.Latitude").AttemptedValue, CultureInfo.GetCultureInfo("en-us"));
            var longitude = double.Parse(bindingContext.ValueProvider.GetValue("Coordinates.Longitude").AttemptedValue, CultureInfo.GetCultureInfo("en-us"));
            return new GeoLocation(latitude, longitude);
        }
    }
}