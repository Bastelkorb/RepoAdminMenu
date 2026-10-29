using MenuLib.MonoBehaviors;
using MenuLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RepoAdminMenu.Utils;
using REPOLib.Modules;

namespace RepoAdminMenu {
    internal class Menu {

        private static string selectedPlayerId;
        private static REPOPopupPage currentMenu;
        private static string currentMenuStr = "";
        private static readonly Dictionary<string, Action> menus = new Dictionary<string, Action>();
        private static readonly MethodInfo removeAllPagesMethod = AccessTools.Method(typeof(MenuManager), "PageCloseAll");
        private static readonly Dictionary<string, Action<string, REPOPopupPage>> menuPreCallbacks = new Dictionary<string, Action<string, REPOPopupPage>>();
        private static readonly Dictionary<string, Action<string, REPOPopupPage>> menuPostCallbacks = new Dictionary<string, Action<string, REPOPopupPage>>();

        public static void Init() {
            if (menus.Count > 0) {
                return;
            }

            registerMenu("playerList", openPlayerListMenu);
            registerMenu("player", openPlayerMenu);
            registerMenu("playerUpgrade", openPlayerUpgrades);
            registerMenu("spawn", openSpawnMenu);
            registerMenu("spawnItem", openSpawnItemsMenu);
            registerMenu("spawnEnemy", openSpawnEnemyMenu);
            registerMenu("spawnValuable", openSpawnValuablesMenu);
            registerMenu("spawnValuableTiny", openSpawnValuablesTinyMenu);
            registerMenu("spawnValuableSmall", openSpawnValuablesSmallMenu);
            registerMenu("spawnValuableMedium", openSpawnValuablesMediumMenu);
            registerMenu("spawnValuableBig", openSpawnValuablesBigMenu);
            registerMenu("spawnValuableWide", openSpawnValuablesWideMenu);
            registerMenu("spawnValuableTall", openSpawnValuablesTallMenu);
            registerMenu("spawnValuableVeryTall", openSpawnValuablesVeryTallMenu);
            registerMenu("map", openMapMenu);
            registerMenu("levelSelector", openLevelSelectorMenu);
            registerMenu("settings", openSettingsMenu);
            registerMenu("credits", openCreditsMenu);
            registerMenu("mainmenu", openMainMenu);
        }

        public static void registerMenu(string name, Action openAction) {
            if (string.IsNullOrWhiteSpace(name) || openAction == null) {
                return;
            }

            if (menus.ContainsKey(name)) {
                menus[name] = openAction;
                return;
            }

            menus.Add(name, openAction);
        }

        public static void addMenuPreCallback(string modName, Action<string, REPOPopupPage> action) {
            if (string.IsNullOrWhiteSpace(modName) || action == null) {
                return;
            }

            if (menuPreCallbacks.ContainsKey(modName)) {
                menuPreCallbacks[modName] = action;
                return;
            }

            menuPreCallbacks.Add(modName, action);
        }

        public static void removeMenuPreCallback(string modName) {
            if (string.IsNullOrWhiteSpace(modName)) {
                return;
            }

            menuPreCallbacks.Remove(modName);
        }

        public static void addMenuPostCallback(string modName, Action<string, REPOPopupPage> action) {
            if (string.IsNullOrWhiteSpace(modName) || action == null) {
                return;
            }

            if (menuPostCallbacks.ContainsKey(modName)) {
                menuPostCallbacks[modName] = action;
                return;
            }

            menuPostCallbacks.Add(modName, action);
        }

        public static void removeMenuPostCallback(string modName) {
            if (string.IsNullOrWhiteSpace(modName)) {
                return;
            }

            menuPostCallbacks.Remove(modName);
        }

        public static void toggleMenu() {
            if (menus.Count < 1) {
                Init();
            }

            if (SemiFunc.MenuLevel()) {
                RepoAdminMenu.mls.LogInfo("Repo Admin Menu can only be opened while in-game!");
                return;
            }

            if (!SemiFunc.IsMasterClientOrSingleplayer()) {
                RepoAdminMenu.mls.LogInfo("Repo Admin Menu can only be opened in single player or as host in multiplayer!");
                return;
            }

            if (currentMenu != null && currentMenu.isActiveAndEnabled) {
                closePage(currentMenu);
            } else {
                RepoAdminMenu.mls.LogInfo("Opening menu");
                menus.GetValueOrDefault(currentMenuStr, openMainMenu).Invoke();
            }
        }

        public static void closePage(REPOPopupPage page) {
            if (page == null) {
                return;
            }

            RepoAdminMenu.mls.LogInfo("Closing: " + page.menuPage.name);
            if (removeAllPagesMethod != null) {
                removeAllPagesMethod.Invoke(MenuManager.instance, new object[] { });
            }

            page.ClosePage(true);
            if (MenuManager.instance != null) {
                MenuManager.instance.PageRemove(page.menuPage);
            }

            if (currentMenu == page) {
                currentMenu = null;
            }
        }

        public static void navigate(REPOPopupPage page, string menu) {
            if (page == null) {
                return;
            }

            closePage(page);
            if (!string.IsNullOrWhiteSpace(menu) && menus.ContainsKey(menu)) {
                menus[menu].Invoke();
                return;
            }

            RepoAdminMenu.mls.LogError("Menu not found: " + menu);
        }

        public static void openPage(REPOPopupPage page, string name) {
            if (page == null) {
                return;
            }

            MenuManager.instance.StartCoroutine(openPageInternal(page, name));
        }

        private static System.Collections.IEnumerator openPageInternal(REPOPopupPage page, string name) {
            yield return new WaitForSeconds(0.050f);

            foreach (KeyValuePair<string, Action<string, REPOPopupPage>> entry in menuPostCallbacks) {
                RepoAdminMenu.mls.LogInfo("Running post-callback for '" + entry.Key + "' on menu '" + name + "'");
                entry.Value.Invoke(name, page);
            }

            RepoAdminMenu.mls.LogInfo("Opening: " + page.menuPage.name);
            if (removeAllPagesMethod != null) {
                removeAllPagesMethod.Invoke(MenuManager.instance, new object[] { });
            }

            page.OpenPage(false);
            currentMenu = page;
            currentMenuStr = name;
        }

        public static REPOPopupPage createMenu(string title, string currentMenu, string parentMenu) {
            var elem = MenuAPI.CreateREPOPopupPage(title, REPOPopupPage.PresetSide.Left, false, true);
            foreach (KeyValuePair<string, Action<string, REPOPopupPage>> entry in menuPreCallbacks) {
                RepoAdminMenu.mls.LogInfo("Running pre-callback for '" + entry.Key + "' on menu '" + currentMenu + "'");
                entry.Value.Invoke(currentMenu, elem);
            }
            addBackButton(elem, parentMenu);
            return elem;
        }

        public static REPOPopupPage createMainMenu(string title) {
            var elem = MenuAPI.CreateREPOPopupPage(title, REPOPopupPage.PresetSide.Left, false, true);
            foreach (KeyValuePair<string, Action<string, REPOPopupPage>> entry in menuPreCallbacks) {
                RepoAdminMenu.mls.LogInfo("Running pre-callback for '" + entry.Key + "' on menu 'mainmenu'");
                entry.Value.Invoke("mainmenu", elem);
            }
            addCloseButton(elem);
            return elem;
        }

        public static void addButton(REPOPopupPage parent, string text, Action action) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOButton(text, action, scrollView);
                return elem.rectTransform;
            });
        }

        public static void addBackButton(REPOPopupPage parent, string parentMenu) {
            if (parent == null) {
                return;
            }

            parent.AddElement(transform => {
                MenuAPI.CreateREPOButton("← Back", () => {
                    closePage(parent);
                    if (menus.ContainsKey(parentMenu)) {
                        menus[parentMenu].Invoke();
                        return;
                    }

                    RepoAdminMenu.mls.LogError("Menu not found: " + parentMenu);
                }, transform, new Vector2(0, 0));
            });
        }

        public static void addCloseButton(REPOPopupPage parent) {
            if (parent == null) {
                return;
            }

            parent.AddElement(transform => {
                MenuAPI.CreateREPOButton("Close", () => closePage(parent), transform, new Vector2(270, 20));
            });
        }

        public static void addToggle(REPOPopupPage parent, string text, Action<bool> action, bool defaultValue, string[] buttonText = null) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOToggle(text, action, scrollView, Vector2.zero, buttonText == null || buttonText.Length < 2 ? "Off" : buttonText[0], buttonText == null || buttonText.Length < 2 ? "On" : buttonText[1], defaultValue, REPOSlider.BarBehavior.UpdateWithValue);
                return elem.rectTransform;
            });
        }

        public static void addLabel(REPOPopupPage parent, string text) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOLabel(text, scrollView);
                return elem.rectTransform;
            });
        }

        public static void addIntSlider(REPOPopupPage parent, string text, string description, Action<int> action, int min, int max, int defaultValue) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOSlider(text, description, action, scrollView, Vector2.zero, min, max, defaultValue, "", "", REPOSlider.BarBehavior.UpdateWithValue);
                return elem.rectTransform;
            });
        }

        public static void addFloatSlider(REPOPopupPage parent, string text, string description, Action<float> action, float min, float max, int precision, float defaultValue) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOSlider(text, description, action, scrollView, Vector2.zero, min, max, precision, defaultValue, "", "", REPOSlider.BarBehavior.UpdateWithValue);
                return elem.rectTransform;
            });
        }

        public static void addStringSlider(REPOPopupPage parent, string text, string description, Action<string> action, string[] options, string defaultValue) {
            if (parent == null) {
                return;
            }

            parent.AddElementToScrollView(scrollView => {
                var elem = MenuAPI.CreateREPOSlider(text, description, action, scrollView, options, defaultValue, Vector2.zero, "", "", REPOSlider.BarBehavior.UpdateWithValue);
                return elem.rectTransform;
            });
        }

        public static void openMainMenu() {
            var mainMenu = createMainMenu("R.E.P.O. Admin Menu");

            addButton(mainMenu, "Players", () => { navigate(mainMenu, "playerList"); });
            addButton(mainMenu, "Spawn", () => { navigate(mainMenu, "spawn"); });
            addButton(mainMenu, "Map", () => { navigate(mainMenu, "map"); });
            addButton(mainMenu, "Settings", () => { navigate(mainMenu, "settings"); });
            addButton(mainMenu, "Credits", () => { navigate(mainMenu, "credits"); });
            addButton(mainMenu, "Report a Bug", () => {
                Application.OpenURL("https://github.com/proferabg/RepoAdminMenu/issues/new?template=bug_report.md");
                Application.OpenURL("file://" + Application.dataPath);
            });

            openPage(mainMenu, "mainmenu");
        }

        private static void openPlayerListMenu() {
            var playersMenu = createMenu("R.A.M. - Players", "playerList", "mainmenu");

            foreach (PlayerAvatar player in SemiFunc.PlayerGetAll()) {
                addButton(playersMenu, SemiFunc.PlayerGetName(player), () => {
                    selectedPlayerId = SemiFunc.PlayerGetSteamID(player);
                    navigate(playersMenu, "player");
                });
            }

            openPage(playersMenu, "playerList");
        }

        private static void openPlayerMenu() {
            if (string.IsNullOrEmpty(selectedPlayerId) || SemiFunc.PlayerGetFromSteamID(selectedPlayerId) == null) {
                RepoAdminMenu.mls.LogInfo("No player selected");
                openPlayerListMenu();
                return;
            }

            PlayerAvatar avatar = SemiFunc.PlayerGetFromSteamID(selectedPlayerId);
            var playerMenu = createMenu("R.A.M. - " + SemiFunc.PlayerGetName(avatar), "player", "playerList");

            addToggle(playerMenu, "God Mode", (b) => { Settings.toggle(Settings.instance.godModePlayers, avatar.steamID, b); }, Settings.isGod(avatar));
            addToggle(playerMenu, "No Death", (b) => { Settings.toggle(Settings.instance.noDeathPlayers, avatar.steamID, b); }, Settings.isNoDeath(avatar));
            addToggle(playerMenu, "No Target", (b) => { Settings.toggle(Settings.instance.noTargetPlayers, avatar.steamID, b); }, Settings.isNoTarget(avatar));
            addToggle(playerMenu, "No Tumble", (b) => { Settings.toggle(Settings.instance.noTumblePlayers, avatar.steamID, b); }, Settings.isNoTumble(avatar));
            addToggle(playerMenu, "Infinite Stamina", (b) => { Settings.toggle(Settings.instance.infiniteStaminaPlayers, avatar.steamID, b); }, Settings.isInfiniteStamina(avatar));
            addToggle(playerMenu, "Force Tumble", (b) => { Settings.toggleDictLong(Settings.instance.forcedTumble, avatar.steamID, b); }, Settings.isForceTumble(avatar));
            addButton(playerMenu, "Upgrades", () => { navigate(playerMenu, "playerUpgrade"); });
            addButton(playerMenu, "Heal", () => { PlayerUtil.healPlayer(avatar); });
            addButton(playerMenu, "Kill", () => { PlayerUtil.killPlayer(avatar); });
            addButton(playerMenu, "Revive", () => { PlayerUtil.revivePlayer(avatar); });
            addButton(playerMenu, "Teleport To Player", () => { PlayerUtil.teleportTo(avatar); });
            addButton(playerMenu, "Summon", () => { PlayerUtil.summon(avatar); });
            addButton(playerMenu, "Return To Truck", () => { PlayerUtil.returnToTruck(avatar); });
            addButton(playerMenu, "Give Crown", () => { PlayerUtil.giveCrown(avatar); });
            addButton(playerMenu, "Kick Player", () => {
                closePage(playerMenu);
                openConfirmMenu("player", "Kick: " + SemiFunc.PlayerGetName(avatar), new Dictionary<string, Action> {
                    { "Yes", () => { PlayerUtil.KickPlayer(avatar); } },
                    { "No", null }
                });
            });

            addButton(playerMenu, "Ban Player", () => {
                closePage(playerMenu);
                openConfirmMenu("player", "Ban: " + SemiFunc.PlayerGetName(avatar), new Dictionary<string, Action> {
                    { "Yes", () => { PlayerUtil.BanPlayer(avatar); } },
                    { "No", null }
                });
            });

            openPage(playerMenu, "player");
        }

        private static void openPlayerUpgrades() {
            if (string.IsNullOrEmpty(selectedPlayerId) || SemiFunc.PlayerGetFromSteamID(selectedPlayerId) == null) {
                RepoAdminMenu.mls.LogInfo("No player selected");
                openPlayerListMenu();
                return;
            }

            PlayerAvatar avatar = SemiFunc.PlayerGetFromSteamID(selectedPlayerId);
            var upgradesMenu = createMenu("R.A.M. - " + SemiFunc.PlayerGetName(avatar) + " - Upgrades", "playerUpgrade", "player");

            foreach (KeyValuePair<string, UpgradeUtil.GameUpgrade> upgradePair in UpgradeUtil.GetGameUpgrades()) {
                addIntSlider(upgradesMenu, upgradePair.Value.GetName(), string.Empty, v => { upgradePair.Value.Upgrade(avatar, v); }, 0, Configuration.MaxUpgradeLevel.Value, upgradePair.Value.GetPlayerLevel(avatar));
            }

            foreach (KeyValuePair<string, PlayerUpgrade> upgradePair in UpgradeUtil.GetModUpgrades()) {
                addIntSlider(upgradesMenu, "(Mod) " + upgradePair.Key, string.Empty, v => { UpgradeUtil.UpgradeLevel(upgradePair.Value, avatar, v); }, 0, Configuration.MaxUpgradeLevel.Value, UpgradeUtil.GetPlayerUpgradeLevel(upgradePair.Value.UpgradeId, avatar));
            }

            openPage(upgradesMenu, "playerUpgrade");
        }

        private static void openSpawnMenu() {
            var spawnerMenu = createMenu("R.A.M. - Spawn", "spawn", "mainmenu");

            addButton(spawnerMenu, "Items", () => { navigate(spawnerMenu, "spawnItem"); });
            addButton(spawnerMenu, "Valuables", () => { navigate(spawnerMenu, "spawnValuable"); });
            addButton(spawnerMenu, "Enemies", () => { navigate(spawnerMenu, "spawnEnemy"); });

            openPage(spawnerMenu, "spawn");
        }

        private static void openSpawnItemsMenu() {
            var itemsMenu = createMenu("R.A.M. - Spawn - Items", "spawnItem", "spawn");

            foreach (KeyValuePair<string, Item> entry in ItemUtil.getItems()) {
                addButton(itemsMenu, entry.Key, () => ItemUtil.spawnItem(entry.Value));
            }

            openPage(itemsMenu, "spawnItem");
        }

        private static void openSpawnValuablesMenu() {
            var valuablesMenu = createMenu("R.A.M. - Spawn - Valuables", "spawnValuable", "spawn");

            addButton(valuablesMenu, "Tiny", () => { navigate(valuablesMenu, "spawnValuableTiny"); });
            addButton(valuablesMenu, "Small", () => { navigate(valuablesMenu, "spawnValuableSmall"); });
            addButton(valuablesMenu, "Medium", () => { navigate(valuablesMenu, "spawnValuableMedium"); });
            addButton(valuablesMenu, "Big", () => { navigate(valuablesMenu, "spawnValuableBig"); });
            addButton(valuablesMenu, "Wide", () => { navigate(valuablesMenu, "spawnValuableWide"); });
            addButton(valuablesMenu, "Tall", () => { navigate(valuablesMenu, "spawnValuableTall"); });
            addButton(valuablesMenu, "Very Tall", () => { navigate(valuablesMenu, "spawnValuableVeryTall"); });

            openPage(valuablesMenu, "spawnValuable");
        }

        private static void openSpawnValuablesTinyMenu() {
            var valuablesTinyMenu = createMenu("R.A.M. - Tiny Valuables", "spawnValuableTiny", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getTinyValuables()) {
                addButton(valuablesTinyMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesTinyMenu, "spawnValuableTiny");
        }

        private static void openSpawnValuablesSmallMenu() {
            var valuablesSmallMenu = createMenu("R.A.M. - Small Valuables", "spawnValuableSmall", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getSmallValuables()) {
                addButton(valuablesSmallMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesSmallMenu, "spawnValuableSmall");
        }

        private static void openSpawnValuablesMediumMenu() {
            var valuablesMediumMenu = createMenu("R.A.M. - Medium Valuables", "spawnValuableMedium", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getMediumValuables()) {
                addButton(valuablesMediumMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesMediumMenu, "spawnValuableMedium");
        }

        private static void openSpawnValuablesBigMenu() {
            var valuablesBigMenu = createMenu("R.A.M. - Big Valuables", "spawnValuableBig", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getBigValuables()) {
                addButton(valuablesBigMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesBigMenu, "spawnValuableBig");
        }

        private static void openSpawnValuablesWideMenu() {
            var valuablesWideMenu = createMenu("R.A.M. - Wide Valuables", "spawnValuableWide", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getWideValuables()) {
                addButton(valuablesWideMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesWideMenu, "spawnValuableWide");
        }

        private static void openSpawnValuablesTallMenu() {
            var valuablesTallMenu = createMenu("R.A.M. - Tall Valuables", "spawnValuableTall", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getTallValuables()) {
                addButton(valuablesTallMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesTallMenu, "spawnValuableTall");
        }

        private static void openSpawnValuablesVeryTallMenu() {
            var valuablesVeryTallMenu = createMenu("R.A.M. - Very Tall Valuables", "spawnValuableVeryTall", "spawnValuable");

            foreach (KeyValuePair<string, PrefabRef> entry in ValuableUtil.getVeryTallValuables()) {
                addButton(valuablesVeryTallMenu, entry.Key, () => ValuableUtil.spawnValuable(entry.Value));
            }

            openPage(valuablesVeryTallMenu, "spawnValuableVeryTall");
        }

        private static void openSpawnEnemyMenu() {
            var spawnerMenu = createMenu("R.A.M. - Spawn - Enemies", "spawnEnemy", "spawn");

            foreach (string enemy in EnemyUtil.getEnemies().Keys) {
                addButton(spawnerMenu, enemy, () => EnemyUtil.spawnEnemy(enemy));
            }

            foreach (string enemy in EnemyUtil.getModEnemies().Keys) {
                addButton(spawnerMenu, "(Mod) " + enemy, () => EnemyUtil.spawnEnemy(enemy));
            }

            openPage(spawnerMenu, "spawnEnemy");
        }

        private static void openMapMenu() {
            var mapMenu = createMenu("R.A.M. - Map Settings", "map", "mainmenu");

            foreach (KeyValuePair<string, Level> entry in MapUtil.getMaps()) {
                addToggle(mapMenu, entry.Key, b => { MapUtil.setMapEnabled(entry.Value, b); }, MapUtil.isMapEnabled(entry.Value));
            }

            addIntSlider(mapMenu, "Current Level", string.Empty, v => { RunManager.instance.levelsCompleted = v; }, 1, 500, RunManager.instance.levelsCompleted);
            addIntSlider(mapMenu, "Money", string.Empty, v => { PunManager.instance.SetRunStatSet("currency", v); }, 0, 9999, StatsManager.instance.runStats["currency"]);
            addButton(mapMenu, "Map Selector", () => { navigate(mapMenu, "levelSelector"); });
            addButton(mapMenu, "Discover Extraction Point", ExtractionPointUtil.discoverNext);
            addButton(mapMenu, "Complete Extraction Point", ExtractionPointUtil.complete);

            openPage(mapMenu, "map");
        }

        private static void openLevelSelectorMenu() {
            var mapMenu = createMenu("R.A.M. - Map Selector", "levelSelector", "map");

            foreach (KeyValuePair<string, Level> entry in MapUtil.getMaps()) {
                addButton(mapMenu, entry.Key, () => { MapUtil.changeLevel(entry.Value); });
            }

            foreach (KeyValuePair<string, Level> entry in MapUtil.getArenas()) {
                addButton(mapMenu, entry.Key, () => { MapUtil.changeLevel(entry.Value); });
            }

            addButton(mapMenu, "Lobby", () => { MapUtil.changeLevel(RunManager.instance.levelLobby); });

            foreach (KeyValuePair<string, Level> entry in MapUtil.getShops()) {
                addButton(mapMenu, entry.Key, () => { MapUtil.changeLevel(entry.Value); });
            }

            openPage(mapMenu, "levelSelector");
        }

        private static void openSettingsMenu() {
            var settingsMenu = createMenu("R.A.M. - Settings", "settings", "mainmenu");

            addToggle(settingsMenu, "Infinite Money", b => { Settings.UpdateOption(ref Settings.instance.infiniteMoney, b); }, Settings.instance.infiniteMoney);
            addToggle(settingsMenu, "No Break", b => { Settings.UpdateOption(ref Settings.instance.noBreak, b); }, Settings.instance.noBreak);
            addToggle(settingsMenu, "No Battery/Ammo Drain", b => { Settings.UpdateOption(ref Settings.instance.noBatteryDrain, b); }, Settings.instance.noBatteryDrain);
            addToggle(settingsMenu, "No Traps", b => { Settings.UpdateOption(ref Settings.instance.noTraps, b); }, Settings.instance.noTraps);
            addToggle(settingsMenu, "Weak Enemies", b => { Settings.UpdateOption(ref Settings.instance.weakEnemies, b); }, Settings.instance.weakEnemies);
            addToggle(settingsMenu, "Deaf Enemies", b => { Settings.UpdateOption(ref Settings.instance.deafEnemies, b); }, Settings.instance.deafEnemies);
            addToggle(settingsMenu, "Blind Enemies", b => { Settings.UpdateOption(ref Settings.instance.blindEnemies, b); }, Settings.instance.blindEnemies);
            addToggle(settingsMenu, "Boom Hammer", b => { Settings.UpdateOption(ref Settings.instance.boomhammer, b); }, Settings.instance.boomhammer);
            addToggle(settingsMenu, "Friendly Duck", b => { Settings.UpdateOption(ref Settings.instance.friendlyDuck, b); }, Settings.instance.friendlyDuck);
            addToggle(settingsMenu, "Upgrade In Shop", b => { Settings.UpdateOption(ref Settings.instance.useShopUpgrades, b); }, Settings.instance.useShopUpgrades);

            openPage(settingsMenu, "settings");
        }

        private static void openCreditsMenu() {
            var creditsMenu = createMenu("R.A.M. - Credits", "credits", "mainmenu");

            addLabel(creditsMenu, "Repo Admin Menu by");
            addLabel(creditsMenu, "  proferabg  ");
            addLabel(creditsMenu, "");
            addLabel(creditsMenu, "Special thanks to:");
            addLabel(creditsMenu, " - REPOrium_Team");
            addLabel(creditsMenu, " - nickklmao");
            addLabel(creditsMenu, " - Godji");
            addLabel(creditsMenu, " - Zehs");
            addLabel(creditsMenu, "");
            addLabel(creditsMenu, "Repo Admin Menu © 2025");

            openPage(creditsMenu, "credits");
        }

        private static void openConfirmMenu(string currentMenu, string message, Dictionary<string, Action> options) {
            var confirmMenu = createMenu("R.A.M. - Confirm", "confirm", "player");
            confirmMenu.closeMenuOnEscape = false;

            addLabel(confirmMenu, message);
            addLabel(confirmMenu, "");
            foreach (KeyValuePair<string, Action> option in options) {
                addButton(confirmMenu, option.Key, () => {
                    if (option.Value != null) {
                        option.Value.Invoke();
                    }
                    navigate(confirmMenu, currentMenu);
                });
            }

            openPage(confirmMenu, "confirm");
        }

        public static string getSelectedPlayer() {
            return selectedPlayerId;
        }
    }
}
