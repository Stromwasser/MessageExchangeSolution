namespace MessageExchangeClient
{
    public class MessageDTO
    {
        public int SequenceNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}
