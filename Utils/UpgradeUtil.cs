using Newtonsoft.Json;
using Photon.Realtime;
using RepoAdminMenu.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RepoAdminMenu {
    internal class Settings {

        internal static Settings instance;

        public Settings() {
            instance = this;
        }

        public static void Init() {
            if (instance == null) {
                instance = new Settings();
            }
        }

        public bool infiniteMoney = false,
            noBreak = false,
            noBatteryDrain = false,
            noTraps = false,
            weakEnemies = false,
            deafEnemies = false,
            blindEnemies = false,
            boomhammer = false,
            friendlyDuck = false,
            useShopUpgrades = false;

        public List<string> godModePlayers = new List<string>(),
            noDeathPlayers = new List<string>(),
            infiniteStaminaPlayers = new List<string>(),
            noTargetPlayers = new List<string>(),
            noTumblePlayers = new List<string>();

        public Dictionary<string, long> forcedTumble = new Dictionary<string, long>();

        public static void toggle(List<string> list, string steamId, bool enabled) {
            if (list == null || string.IsNullOrEmpty(steamId)) {
                return;
            }

            if (enabled) {
                if (!list.Contains(steamId)) {
                    list.Add(steamId);
                }
            } else {
                list.Remove(steamId);
            }

            UpdateClients();
        }

        public static void toggleDictLong(Dictionary<string, long> list, string steamId, bool enabled) {
            if (list == null || string.IsNullOrEmpty(steamId)) {
                return;
            }

            if (enabled) {
                if (!list.ContainsKey(steamId)) {
                    list[steamId] = 0L;
                }
            } else {
                list.Remove(steamId);
            }

            UpdateClients();
        }

        public static bool isGod(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.godModePlayers.Contains(SemiFunc.PlayerGetSteamID(avatar));
        }

        public static bool isNoDeath(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.noDeathPlayers.Contains(SemiFunc.PlayerGetSteamID(avatar));
        }

        public static bool isInfiniteStamina(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.infiniteStaminaPlayers.Contains(SemiFunc.PlayerGetSteamID(avatar));
        }

        public static bool isNoTarget(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.noTargetPlayers.Contains(SemiFunc.PlayerGetSteamID(avatar));
        }

        public static bool isNoTumble(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.noTumblePlayers.Contains(SemiFunc.PlayerGetSteamID(avatar));
        }

        public static bool isForceTumble(PlayerAvatar avatar) {
            return avatar != null && instance != null && instance.forcedTumble.ContainsKey(SemiFunc.PlayerGetSteamID(avatar));
        }

        internal static long getLastForceTumble(PlayerAvatar avatar) {
            if (avatar == null || instance == null) {
                return 0L;
            }

            return instance.forcedTumble.GetValueOrDefault(SemiFunc.PlayerGetSteamID(avatar), 0L);
        }

        internal static long setLastForceTumble(PlayerAvatar avatar, long lastTumble) {
            if (avatar == null || instance == null) {
                return 0L;
            }

            return instance.forcedTumble[SemiFunc.PlayerGetSteamID(avatar)] = lastTumble;
        }

        public static void UpdateOption(ref bool option, bool value) {
            option = value;
            UpdateClients();
        }

        public static void UpdateClients() {
            if (instance == null || !SemiFunc.IsMasterClient()) {
                return;
            }

            RepoAdminMenu.mls.LogInfo("Sending Settings Sync");
            NetworkUtil.SendCommandString("SettingsSync", instance.toJson(), ReceiverGroup.Others);
        }

        public string toJson() {
            return JsonConvert.SerializeObject(this);
        }

        public static void loadFromJson(string json) {
            if (string.IsNullOrWhiteSpace(json)) {
                return;
            }

            var loaded = JsonConvert.DeserializeObject<Settings>(json);
            if (loaded != null) {
                instance = loaded;
            }
        }

        internal void log() {
            RepoAdminMenu.mls.LogInfo("\n\n\n" + instance.toJson() + "\n\n\n");
            RepoAdminMenu.mls.LogInfo("Current Settings:\n" +
                "  infiniteMoney - " + instance.infiniteMoney + "\n" +
                "  noBreak - " + instance.noBreak + "\n" +
                "  noBatteryDrain - " + instance.noBatteryDrain + "\n" +
                "  noTraps - " + instance.noTraps + "\n" +
                "  weakEnemies - " + instance.weakEnemies + "\n" +
                "  deafEnemies - " + instance.deafEnemies + "\n" +
                "  blindEnemies - " + instance.blindEnemies + "\n" +
                "  boomhammer - " + instance.boomhammer + "\n" +
                "  friendlyDuck - " + instance.friendlyDuck + "\n" +
                "  useShopUpgrades - " + instance.useShopUpgrades + "\n" +
                "  godMode: - " + string.Join(",", instance.godModePlayers.Select(player => SemiFunc.PlayerGetName(SemiFunc.PlayerGetFromSteamID(player)))) + "\n" +
                "  noDeathPlayers: - " + string.Join(",", instance.noDeathPlayers.Select(player => SemiFunc.PlayerGetName(SemiFunc.PlayerGetFromSteamID(player)))) + "\n" +
                "  infiniteStaminaPlayers: - " + string.Join(",", instance.infiniteStaminaPlayers.Select(player => SemiFunc.PlayerGetName(SemiFunc.PlayerGetFromSteamID(player)))) + "\n" +
                "  noTargetPlayers: - " + string.Join(",", instance.noTargetPlayers.Select(player => SemiFunc.PlayerGetName(SemiFunc.PlayerGetFromSteamID(player)))) + "\n" +
                "  noTumblePlayers: - " + string.Join(",", instance.noTumblePlayers.Select(player => SemiFunc.PlayerGetName(SemiFunc.PlayerGetFromSteamID(player)))) + "\n");
        }
    }
}
