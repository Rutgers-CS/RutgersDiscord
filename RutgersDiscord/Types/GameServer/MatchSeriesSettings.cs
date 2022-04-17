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
    private string map1_start_ct { get; set; }
    private string map2 { get; set; }
    private string map2_start_ct { get; set; }
    private string map3 { get; set; }
    private string map3_start_ct { get; set; }
    private string map4 { get; set; }
    private string map4_start_ct { get; set; }
    private string map5 { get; set; }
    private string map5_start_ct { get; set; }
    private string match_end_webhook_url { get; set; }
    private string match_series_end_webhook_url { get; set; }
    private string message_prefix { get; set; } = "Scarlet Classic";
    private int number_of_maps { get; set; }
    private string playwin_result_webhook_url { get; set; }
    private int ready_min_players { get; set; } = 2;
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

    public MatchSeriesSettings ()
    {

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
            //TODO Finish Values
        });
    }
}
