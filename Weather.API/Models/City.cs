namespace Weather.API.Models
{
    public class City
    {
        public float Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public Coord Coord { get; set; }
    }
}
