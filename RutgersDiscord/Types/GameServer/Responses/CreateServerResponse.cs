using System.Collections.Generic;

namespace RutgersDiscord.Types.GameServer.Responses
{
    public class CreateServerCsgoSettings
    {
        public List<string> autoload_configs { get; set; }
        public bool disable_1v1_warmup_arenas { get; set; }
        public bool disable_bots { get; set; }
        public bool enable_csay_plugin { get; set; }
        public bool enable_gotv { get; set; }
        public bool enable_gotv_secondary { get; set; }
        public bool enable_sourcemod { get; set; }
        public string game_mode { get; set; }
        public bool insecure { get; set; }
        public string mapgroup { get; set; }
        public string mapgroup_start_map { get; set; }
        public string maps_source { get; set; }
        public string password { get; set; }
        public bool private_server { get; set; }
        public bool pure_server { get; set; }
        public string rcon { get; set; }
        public int slots { get; set; }
        public string sourcemod_admins { get; set; }
        public List<string> sourcemod_plugins { get; set; }
        public string steam_game_server_login_token { get; set; }
        public int tickrate { get; set; }
        public string workshop_authkey { get; set; }
        public string workshop_id { get; set; }
        public string workshop_start_map_id { get; set; }
    }

    public class CreateServerMumbleSettings
    {
        public string password { get; set; }
        public int slots { get; set; }
        public string superuser_password { get; set; }
        public string welcome_text { get; set; }
    }

    public class CreateServerPorts
    {
        public int game { get; set; }
        public int gotv { get; set; }
        public int gotv_secondary { get; set; }
        public int query { get; set; }
    }

    public class CreateServerScheduledCommand
    {
        public string action { get; set; }
        public string command { get; set; }
        public string name { get; set; }
        public int repeat { get; set; }
        public int run_at { get; set; }
    }

    public class CreateServerStatus
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class CreateServerTeamfortress2Settings
    {
        public bool enable_gotv { get; set; }
        public bool enable_sourcemod { get; set; }
        public bool insecure { get; set; }
        public string password { get; set; }
        public string rcon { get; set; }
        public int slots { get; set; }
        public string sourcemod_admins { get; set; }
    }

    public class CreateServerTeamspeak3Settings
    {
        public int slots { get; set; }
        public string ts_admin_token { get; set; }
        public string ts_server_id { get; set; }
    }

    public class CreateServerValheimSettings
    {
        public List<string> admins_steamid64 { get; set; }
        public List<string> bepinex_plugins { get; set; }
        public bool enable_bepinex { get; set; }
        public bool enable_valheimplus { get; set; }
        public string password { get; set; }
        public int slots { get; set; }
        public string world_name { get; set; }
    }

    public class CreateServerResponse
    {
        public string added_voice_server { get; set; }
        public bool autostop { get; set; }
        public int autostop_minutes { get; set; }
        public bool booting { get; set; }
        public bool confirmed { get; set; }
        public int cost_per_hour { get; set; }
        public CreateServerCsgoSettings csgo_settings { get; set; }
        public string custom_domain { get; set; }
        public int cycle_months_12_discount_percentage { get; set; }
        public int cycle_months_1_discount_percentage { get; set; }
        public int cycle_months_3_discount_percentage { get; set; }
        public List<string> default_file_locations { get; set; }
        public int disk_usage_bytes { get; set; }
        public string duplicate_source_server { get; set; }
        public bool enable_core_dump { get; set; }
        public bool enable_mysql { get; set; }
        public bool enable_syntropy { get; set; }
        public int first_month_discount_percentage { get; set; }
        public string ftp_password { get; set; }
        public string game { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
        public string location { get; set; }
        public int manual_sort_order { get; set; }
        public string match_id { get; set; }
        public int max_cost_per_hour { get; set; }
        public int max_cost_per_month { get; set; }
        public int max_disk_usage_gb { get; set; }
        public int month_credits { get; set; }
        public int month_reset_at { get; set; }
        public CreateServerMumbleSettings mumble_settings { get; set; }
        public string mysql_password { get; set; }
        public string mysql_username { get; set; }
        public string name { get; set; }
        public bool on { get; set; }
        public int players_online { get; set; }
        public CreateServerPorts ports { get; set; }
        public bool prefer_dedicated { get; set; }
        public string private_ip { get; set; }
        public string raw_ip { get; set; }
        public bool reboot_on_crash { get; set; }
        public List<CreateServerScheduledCommand> scheduled_commands { get; set; }
        public string server_error { get; set; }
        public string server_image { get; set; }
        public List<CreateServerStatus> status { get; set; }
        public int subscription_cycle_months { get; set; }
        public int subscription_renewal_failed_attempts { get; set; }
        public int subscription_renewal_next_attempt_at { get; set; }
        public string subscription_state { get; set; }
        public CreateServerTeamfortress2Settings teamfortress2_settings { get; set; }
        public CreateServerTeamspeak3Settings teamspeak3_settings { get; set; }
        public string user_data { get; set; }
        public CreateServerValheimSettings valheim_settings { get; set; }
    }
}
