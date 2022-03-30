public class ServerSettings
{
    public string name { get; set; }
    public CSGO_Settings csgo_settings { get; set; }

    public ServerSettings(string _name = null, CSGO_Settings cssettings = null)
    {
        name = _name;
        csgo_settings = cssettings;
    }
}

public class CSGO_Settings
{
    public string steam_game_server_login_token { get; set; }

    public CSGO_Settings(string logintoken = null)
    {
        steam_game_server_login_token = logintoken;
    }
}