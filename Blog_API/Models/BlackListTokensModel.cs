namespace Blog_API.Models
{
    public class BlackListTokensModel
    {
        public int Id { get; set; }

        public string Jti { get; set; }

        public string Token { get; set; }

        public DateTime DateTime { get; set; }
    }
}
