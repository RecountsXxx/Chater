namespace MessengerLiblary
{
    [Serializable]
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string VoiceMessage { get; set; }
        public string ImageMessage { get; set; }
        public string VideoMessage { get; set; }
        public string ZipMessage { get; set; }

        public Message(int Id, DateTime stamp, int senderId, int recipientId)
        {
            this.Id = Id;
            this.TimeStamp = stamp;
            this.SenderId = senderId;
            this.RecipientId = recipientId;

        }
        public enum MessageSide
        {
            Left,
            Right
        }
    }
}