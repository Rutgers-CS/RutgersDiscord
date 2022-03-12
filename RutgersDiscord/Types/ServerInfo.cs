using Dapper.Contrib.Extensions;

public class ServerInfo
{
    //Used to parse database values
    [Key]
    public long ID { get; set; }
    public string Name { get; set; }
    public long Channel { get; set; }
    public string Ip { get; set; }
    public ushort Port { get; set; }
    public string Pass { get; set; }
}
