using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

public class MatchSeriesSettings
{
    public int connect_time { get; set; } = 300;
    public bool enable_pause { get; set; } = true;
    private bool enable_playwin { get; set; } = false; //Requires payment
    public bool enable_ready { get; set; } = true;
    public bool enable_tech_pause { get; set; } = true;
    private string game_server_id { get; set; }
    private string map1 { get; set; }
    private string map1_start_ct { get; set; } = "knife";
    private string map2 { get; set; }
    private string map2_start_ct { get; set; } = "knife";
    private string map3 { get; set; }
    private string map3_start_ct { get; set; } = "knife";
    private string map4 { get; set; }
    private string map4_start_ct { get; set; }
    private string map5 { get; set; }
    private string map5_start_ct { get; set; }
    private string match_end_webhook_url { get; set; }
    private string match_series_end_webhook_url { get; set; }
    private string message_prefix { get; set; } = "Scarlet Classic";
    private int number_of_maps { get; set; } = 3;
    private string playwin_result_webhook_url { get; set; }
    private int ready_min_players { get; set; } = 1; //Change back
    private string round_end_webhook_url { get; set; }
    public string spectator_steam_ids { get; set; }
    private string team1_coach_steam_id { get; set; }
    private string team1_flag { get; set; }
    private string team1_name { get; set; }
    private string team1_steam_ids { get; set; }
    private string team2_coach_steam_id { get; set; }
    private string team2_flag { get; set; }
    private string team2_name { get; set; }
    private string team2_steam_ids { get; set; }
    private int team_size { get; set; }
    private bool wait_for_coaches { get; set; } = false;
    private bool wait_for_gotv_before_nextmap { get; set; } = false;
    private bool wait_for_spectators { get; set; } = false;
    public int warmup_time { get; set; } = 15;

    public MatchSeriesSettings(MapInfo map1, MapInfo map2, MapInfo map3, TeamInfo homeTeam, PlayerInfo homeTeamPlayer1, PlayerInfo homeTeamPlayer2, TeamInfo awayTeam, PlayerInfo awayTeamPlayer1, PlayerInfo awayTeamPlayer2, string gameServerID, string webHookURL)
    {
        game_server_id = gameServerID;
        match_end_webhook_url = webHookURL;

        //Dirty bad code
        if (map1.OfficialMap)
        {
            this.map1 = map1.OfficialID;
        }
        else
        {
            this.map1 = $"workshop/{map1.WorkshopID}";
        }
        if (map2.OfficialMap)
        {
            this.map2 = map1.OfficialID;
        }
        else
        {
            this.map2 = $"workshop/{map2.WorkshopID}";
        }
        if (map3.OfficialMap)
        {
            this.map3 = map1.OfficialID;
        }
        else
        {
            this.map3 = $"workshop/{map3.WorkshopID}";
        }

        team1_name = homeTeam.TeamName;
        team1_steam_ids = $"{homeTeamPlayer1.SteamID}"/*,{homeTeamPlayer2.SteamID}"*/;
        team2_name = awayTeam.TeamName;
        team2_steam_ids = $"{awayTeamPlayer1.SteamID}"/*,{awayTeamPlayer2.SteamID}"*/;
    }

    public FormUrlEncodedContent ToForm()
    {
        return new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("connect_time", connect_time.ToString()),
            new KeyValuePair<string, string>("enable_pause", enable_pause.ToString()),
            new KeyValuePair<string, string>("enable_playwin", enable_playwin.ToString()),
            new KeyValuePair<string, string>("enable_ready", enable_ready.ToString()),
            new KeyValuePair<string, string>("enable_tech_pause", enable_tech_pause.ToString()),
            new KeyValuePair<string, string>("game_server_id", game_server_id),
            new KeyValuePair<string, string>("map1", map1),
            new KeyValuePair<string, string>("map1_start_ct", map1_start_ct),
            new KeyValuePair<string, string>("map2", map2),
            new KeyValuePair<string, string>("map2_start_ct", map2_start_ct),
            new KeyValuePair<string, string>("map3", map3),
            new KeyValuePair<string, string>("map3_start_ct", map3_start_ct),
            new KeyValuePair<string, string>("match_end_webhook_url", match_end_webhook_url),
            new KeyValuePair<string, string>("match_series_end_webhook_url", match_series_end_webhook_url),
            new KeyValuePair<string, string>("message_prefix", message_prefix),
            new KeyValuePair<string, string>("number_of_maps", number_of_maps.ToString()),
            new KeyValuePair<string, string>("ready_min_players", ready_min_players.ToString()),
            new KeyValuePair<string, string>("team1_name", team1_name),
            new KeyValuePair<string, string>("team1_steam_ids", team1_steam_ids),
            new KeyValuePair<string, string>("team2_name", team2_name),
            new KeyValuePair<string, string>("team2_steam_ids", team2_steam_ids),
            new KeyValuePair<string, string>("wait_for_coaches", wait_for_coaches.ToString()),
            new KeyValuePair<string, string>("wait_for_gotv_before_nextmap", wait_for_gotv_before_nextmap.ToString()),
            new KeyValuePair<string, string>("wait_for_spectators", wait_for_spectators.ToString()),
            new KeyValuePair<string, string>("warmup_time", warmup_time.ToString()),
        });
    }
}
