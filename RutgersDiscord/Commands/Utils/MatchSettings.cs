using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands.Utils
{
    public class MatchSettings
    {
        //Default Values (Probably shouldn't be changed)
        private int connect_time { get; set; } = 300;
        private bool enable_knife_round { get; set; } = true;
        private bool enable_pause { get; set; } = true;
        private bool enable_playwin { get; set; } = false; //Requires payment
        private bool enable_ready { get; set; } = false;
        private bool enable_tech_pause { get; set; } = true;
        private string game_server_id { get; set; }
        private string map { get; set; }
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
        private string playwin_result_webhook_url { get; set; }
        private int ready_min_players { get; set; } = 1;
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
        private bool wait_for_coaches { get; set; }
        private bool wait_for_gotv_before_nextmap { get; set; }
        private bool wait_for_spectators { get; set; }
        private int warmup_time { get; set; }
        private string webhook_authorization_header { get; set; }
    }

    /*public MatchSettings()
    {

    }s

    public buildSettigns()
    {

    }*/
}
