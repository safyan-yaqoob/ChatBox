namespace DatingApp.API.Dtos
{
    public class UserForUpdateDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Introduction { get; set; }
        public string LookingFor   { get; set; }
        public string Interest { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}