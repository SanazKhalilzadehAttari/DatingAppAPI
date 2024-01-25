using System.ComponentModel.DataAnnotations.Schema;

namespace DatingAppAPI.Entities
{ 
    [Table("Photos")]
    public class Photo
    {
        public int? Id { get; set; }
        public Uri? Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }

        public int? AppUSerId { get; set; }
        public AppUser AppUser { get; set; }
    }
}