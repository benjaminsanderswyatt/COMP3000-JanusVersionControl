namespace backend.DataTransferObjects.CLI
{
    public class RemoteHeadDto
    {
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public Dictionary<string, string> Heads { get; set; }
    }

}
