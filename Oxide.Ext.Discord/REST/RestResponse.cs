namespace Oxide.Ext.Discord.REST
{
    public class RestResponse
    {
        public string Data { get; }

        public RestResponse(string data)
        {
            this.Data = data;
        }
    }
}
