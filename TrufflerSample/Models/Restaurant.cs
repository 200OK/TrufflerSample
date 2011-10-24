using System.ComponentModel.DataAnnotations;
using Truffler;

namespace TrufflerSample.Models
{
    public class Restaurant
    {
        [Required]
        public string Name { get; set; }

        [Id]
        [Required]
        public string WikipediaUrl { get; set; }
        
        public string WikipediaText { get; set; }
        
        public string Website { get; set; }
        
        public string StreetAddress { get; set; }
        
        public string City { get; set; }
        
        public int? MichelinRating { get; set; }
        
        public string HeadChef { get; set; }

        public string Cuisine { get; set; }
        
        [Required]
        public GeoLocation Coordinates { get; set; }
        
        public string Country { get; set; }
    }
}