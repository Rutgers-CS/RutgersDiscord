using Dapper.Contrib.Extensions;
[Table("servertokens")]
public class ServerTokens
{
    public int id { get; set; }
    public string token { get; set; }
    public int inuse { get; set; }

    private ServerTokens() {}

    public ServerTokens(int id, string token, int inuse)
    {
        this.id = id;
        this.token = token;
        this.inuse = inuse;
    }
}