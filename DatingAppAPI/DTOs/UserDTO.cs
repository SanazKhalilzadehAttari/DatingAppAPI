namespace DatingAppAPI.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public Uri PhotoUrl { get; set; }
        public string KnownAs {  get; set; }
        public string Gender { get; set; }
    }
}
