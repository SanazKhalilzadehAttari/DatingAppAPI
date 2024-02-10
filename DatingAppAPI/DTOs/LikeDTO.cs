namespace DatingAppAPI.DTOs
{
    public class LikeDTO
    {
        public int  Id { get; set; }
        public string Username { get; set; }
        public string city { get; set; }
        public string KnownAs { get; set; }
        public int Age { get; set; }
        public Uri  photoUrl { get; set; }
    }
}
