using System.Collections.Generic;

namespace RutgersDiscord.Types.GameServer.Webhooks
{
    public class MatchTeam1Stats
    {
        public int score { get; set; }
    }

    public class MatchTeam2Stats
    {
        public int score { get; set; }
    }

    public class MatchPlayerStat
    {
        public string steam_id { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
    }

    public class MatchPlaywinResult
    {
    }

    public class MatchEndWebhook
    {
        public string id { get; set; }
        public string game_server_id { get; set; }
        public string map { get; set; }
        public int connect_time { get; set; }
        public int warmup_time { get; set; }
        public bool team1_start_ct { get; set; }
        public List<string> team1_steam_ids { get; set; }
        public object team1_coach_steam_id { get; set; }
        public string team1_name { get; set; }
        public string team1_flag { get; set; }
        public List<string> team2_steam_ids { get; set; }
        public object team2_coach_steam_id { get; set; }
        public string team2_name { get; set; }
        public string team2_flag { get; set; }
        public List<object> spectator_steam_ids { get; set; }
        public bool wait_for_coaches { get; set; }
        public bool wait_for_spectators { get; set; }
        public string round_end_webhook_url { get; set; }
        public string match_end_webhook_url { get; set; }
        public bool started { get; set; }
        public bool finished { get; set; }
        public string cancel_reason { get; set; }
        public int rounds_played { get; set; }
        public MatchTeam1Stats team1_stats { get; set; }
        public MatchTeam2Stats team2_stats { get; set; }
        public List<MatchPlayerStat> player_stats { get; set; }
        public bool enable_knife_round { get; set; }
        public bool enable_pause { get; set; }
        public bool enable_playwin { get; set; }
        public object playwin_result_webhook_url { get; set; }
        public MatchPlaywinResult playwin_result { get; set; }
        public bool enable_ready { get; set; }
        public int ready_min_players { get; set; }
        public bool enable_tech_pause { get; set; }
        public string message_prefix { get; set; }
    }
}
