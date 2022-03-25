using Dapper.Contrib.Extensions;
[Table("servertokens")]
public class ServerTokens
{
    [ExplicitKey]
    public int ID { get; set; }
    public string Token { get; set; }
    public string ServerID { get; set; }

    private ServerTokens() {}

    public ServerTokens(int id, string token, string serverID)
    {
        ID = id;
        Token = token;
        ServerID = serverID;
    }
}