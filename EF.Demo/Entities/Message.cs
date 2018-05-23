namespace EF.Demo.Entities
{
    public class Message
    {
        #region Properties

        public ulong Id { get; set; }

        public Client Client { get; set; }

        public string Content { get; set; }

        #endregion Properties

        public object[] GetKey() => new object[] { Id, Client.Id, Client.Server.Id };
    }
}