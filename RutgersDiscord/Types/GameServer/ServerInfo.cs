public class ServerInfo
{
    public string ServerID { get; }
    public string IP { get; }
    public int Port { get; }

    private ServerInfo() { }
    public ServerInfo(string serverID, string ip, int port)
    {
        ServerID = serverID;
        IP = ip;
        Port = port;
    }

    public override string ToString()
    {
        return $"{IP}:{Port} ({ServerID})";
    }
}
