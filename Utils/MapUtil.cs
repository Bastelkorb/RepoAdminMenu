using Photon.Realtime;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RepoAdminMenu.Utils {
    internal class UpgradeUtil {

        private static readonly SortedDictionary<string, PlayerUpgrade> modUpgrades = new SortedDictionary<string, PlayerUpgrade>();
        private static readonly SortedDictionary<string, GameUpgrade> gameUpgrades = new SortedDictionary<string, GameUpgrade>() {
            { "playerUpgradeCrouchRest", new GameUpgrade("playerUpgradeCrouchRest", "Crouch Rest", PunManager.instance.UpdateCrouchRestRightAway) },
            { "playerUpgradeHealth", new GameUpgrade("playerUpgradeHealth", "Health", PunManager.instance.UpdateHealthRightAway) },
            { "playerUpgradeExtraJump", new GameUpgrade("playerUpgradeExtraJump", "Jump", PunManager.instance.UpdateExtraJumpRightAway) },
            { "playerUpgradeLaunch", new GameUpgrade("playerUpgradeLaunch", "Launch", PunManager.instance.UpdateTumbleLaunchRightAway) },
            { "playerUpgradeMapPlayerCount", new GameUpgrade("playerUpgradeMapPlayerCount", "Map Player Count", PunManager.instance.UpdateMapPlayerCountRightAway) },
            { "playerUpgradeRange", new GameUpgrade("playerUpgradeRange", "Range", PunManager.instance.UpdateGrabRangeRightAway) },
            { "playerUpgradeSpeed", new GameUpgrade("playerUpgradeSpeed", "Speed", PunManager.instance.UpdateSprintSpeedRightAway) },
            { "playerUpgradeStamina", new GameUpgrade("playerUpgradeStamina", "Stamina", PunManager.instance.UpdateEnergyRightAway) },
            { "playerUpgradeStrength", new GameUpgrade("playerUpgradeStrength", "Strength", PunManager.instance.UpdateGrabStrengthRightAway) },
            { "playerUpgradeThrow", new GameUpgrade("playerUpgradeThrow", "Throw", PunManager.instance.UpdateThrowStrengthRightAway) },
            { "playerUpgradeTumbleWings", new GameUpgrade("playerUpgradeTumbleWings", "Tumble Wings", PunManager.instance.UpdateTumbleWingsRightAway) },
            { "playerUpgradeTumbleClimb", new GameUpgrade("playerUpgradeTumbleClimb", "Tumble Climb", PunManager.instance.UpdateTumbleClimbRightAway) },
            { "playerUpgradeDeathHeadBattery", new GameUpgrade("playerUpgradeDeathHeadBattery", "Death Head Battery", PunManager.instance.UpdateDeathHeadBatteryRightAway) },
        };

        public static void Init() {
            modUpgrades.Clear();
            foreach (PlayerUpgrade upgrade in Upgrades.PlayerUpgrades.ToList()) {
                string name = Regex.Replace(upgrade.UpgradeId, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
                if (!modUpgrades.ContainsKey(name)) {
                    modUpgrades.Add(name, upgrade);
                }
            }
        }

        public static SortedDictionary<string, GameUpgrade> GetGameUpgrades() {
            return gameUpgrades;
        }

        public static SortedDictionary<string, PlayerUpgrade> GetModUpgrades() {
            return modUpgrades;
        }

        public static void UpgradeLevel(PlayerUpgrade upgrade, PlayerAvatar avatar, int level) {
            if (upgrade == null || avatar == null) {
                return;
            }

            upgrade.SetLevel(avatar, level);
            RepoAdminMenu.mls.LogInfo("Upgrade: " + SemiFunc.PlayerGetName(avatar) + " - '" + upgrade.UpgradeId + "' -> " + level);
        }

        public static int GetPlayerUpgradeLevel(string type, PlayerAvatar avatar) {
            if (string.IsNullOrEmpty(type) || avatar == null) {
                return 0;
            }

            StatsManager statsManager = StatsManager.instance;
            string playerSteamId = SemiFunc.PlayerGetSteamID(avatar);

            if (statsManager != null && statsManager.dictionaryOfDictionaries.ContainsKey(type)) {
                Dictionary<string, int> upgradeType = statsManager.dictionaryOfDictionaries[type];
                if (upgradeType != null && upgradeType.ContainsKey(playerSteamId)) {
                    return upgradeType[playerSteamId];
                }
            }

            return 0;
        }

        public static void UpgradeSync(PlayerAvatar avatar, string key, int level) {
            if (avatar == null || string.IsNullOrEmpty(key)) {
                return;
            }

            RepoAdminMenu.mls.LogInfo("UpgradeSync: " + SemiFunc.PlayerGetName(avatar) + " - '" + key + "' -> " + level);
            string steamId = SemiFunc.PlayerGetSteamID(avatar);
            SortedDictionary<string, Dictionary<string, int>> dicts = StatsManager.instance.dictionaryOfDictionaries;

            if (!dicts.ContainsKey(key)) {
                dicts.Add(key, new Dictionary<string, int>());
            }

            if (dicts.TryGetValue(key, out var dict)) {
                dict[steamId] = level;
            }

            if (avatar == PlayerAvatar.instance) {
                DoImmediateUpgrade(avatar, key, level);
            }
        }

        private static void DoImmediateUpgrade(PlayerAvatar avatar, string key, int level) {
            if (avatar == null || string.IsNullOrEmpty(key)) {
                return;
            }

            RepoAdminMenu.mls.LogInfo("DoImmediateUpgrade: " + SemiFunc.PlayerGetName(avatar) + " - '" + key + "'");
            try {
                if (gameUpgrades.TryGetValue(key, out var value)) {
                    value.PunUpgrade(avatar, level);
                }
            } catch (Exception ex) {
                RepoAdminMenu.mls.LogWarning("Failed to apply immediate upgrade: " + ex.Message);
            }
        }

        public class GameUpgrade {
            private readonly string id;
            private readonly string name;
            private readonly Action<string, int> punManagerFunction;

            public GameUpgrade(string id, string name, Action<string, int> punManagerFunction) {
                this.id = id;
                this.name = name;
                this.punManagerFunction = punManagerFunction;
            }

            public string GetID() {
                return id;
            }

            public string GetName() {
                return name;
            }

            public int GetPlayerLevel(PlayerAvatar avatar) {
                return GetPlayerUpgradeLevel(id, avatar);
            }

            public void Upgrade(PlayerAvatar avatar, int level) {
                if (avatar == null) {
                    return;
                }

                RepoAdminMenu.mls.LogInfo("Upgrade: " + SemiFunc.PlayerGetName(avatar) + " - '" + id + "' -> " + level);
                if (SemiFunc.IsMultiplayer()) {
                    NetworkUtil.SendCommandSteamIDStringInt("UpgradeSync", avatar.steamID, id, level, ReceiverGroup.All);
                } else {
                    UpgradeSync(avatar, id, level);
                }
            }

            public void PunUpgrade(PlayerAvatar avatar, int level) {
                if (avatar == null || punManagerFunction == null) {
                    return;
                }

                string steamId = SemiFunc.PlayerGetSteamID(avatar);
                punManagerFunction.Invoke(steamId, level);
            }
        }
    }
}
