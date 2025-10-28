namespace BackendEcommerce.Domain.Email
{
    public class EmailMessageDTO
    {
        public string From { get; set; } = "noreply@mail.watchlife.site";
        public string To { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}
