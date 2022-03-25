using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class MatchSettings
{
    //Default Values (Probably shouldn't be changed)
    public int connect_time { get; set; } = 300;
    public bool enable_knife_round { get; set; } = true;
    public bool enable_pause { get; set; } = true;
    private bool enable_playwin { get; set; } = false; //Requires payment
    public bool enable_ready { get; set; } = true;
    public bool enable_tech_pause { get; set; } = true;
    private string game_server_id { get; set; }
    private string map { get; set; }
    private string match_end_webhook_url { get; set; } = $"http://{Environment.GetEnvironmentVariable("publicIP")}:{Environment.GetEnvironmentVariable("port")}/api";
    private string message_prefix { get; set; } = "Scarlet Classic";
    private string playwin_result_webhook_url { get; set; }
    private int ready_min_players { get; set; } = 2;
    private string round_end_webhook_url { get; set; }
    private string spectator_steam_ids { get; set; }
    private string team1_coach_steam_id { get; set; }
    private string team1_flag { get; set; }
    private string team1_name { get; set; }
    private string team1_steam_ids { get; set; }
    private string team2_coach_steam_id { get; set; }
    private string team2_flag { get; set; }
    private string team2_name { get; set; }
    private string team2_steam_ids { get; set; }
    private bool wait_for_coaches { get; set; } = false;
    private bool wait_for_gotv_before_nextmap { get; set; } = false;
    private bool wait_for_spectators { get; set; } = false;
    public int warmup_time { get; set; } = 15;
    private string webhook_authorization_header { get; set; }


    public MatchSettings(MapInfo map, TeamInfo homeTeam, PlayerInfo homeTeamPlayer1, PlayerInfo homeTeamPlayer2, TeamInfo awayTeam, PlayerInfo awayTeamPlayer1, PlayerInfo awayTeamPlayer2, string gameServerID)
    {
        game_server_id = gameServerID;

        if (map.OfficialMap)
        {
            this.map = map.OfficialID;
        }
        else
        {
            this.map = $"workshop/{map.WorkshopID}";
        }

        team1_name = homeTeam.TeamName;
        team1_steam_ids = $"{homeTeamPlayer1.SteamID},{homeTeamPlayer2.SteamID}";
        team2_name = awayTeam.TeamName;
        team2_steam_ids = $"{awayTeamPlayer1.SteamID},{awayTeamPlayer2.SteamID}";
    }

    public string ToJson()
    {
        return JObject.FromObject(this).ToString();
    }
}
