using System.Collections.Generic;

namespace RepoAdminMenu.Utils {
    internal class MapUtil {

        private static readonly SortedDictionary<string, Level> maps = new SortedDictionary<string, Level>();
        private static readonly SortedDictionary<string, Level> arenas = new SortedDictionary<string, Level>();
        private static readonly SortedDictionary<string, Level> shops = new SortedDictionary<string, Level>();
        private static readonly Dictionary<Level, bool> mapToggles = new Dictionary<Level, bool>();
        private static Level nextLevel = null;

        public static void Init() {
            if (RunManager.instance == null) {
                return;
            }

            if (maps.Count > 0 && mapToggles.Count > 0) {
                return;
            }

            maps.Clear();
            arenas.Clear();
            shops.Clear();
            mapToggles.Clear();

            foreach (Level level in RunManager.instance.levels) {
                if (level == null) {
                    continue;
                }

                string key = level.name.Replace("Level - ", string.Empty);
                if (!maps.ContainsKey(key)) {
                    maps.Add(key, level);
                }

                if (!mapToggles.ContainsKey(level)) {
                    mapToggles.Add(level, true);
                }
            }

            foreach (Level level in RunManager.instance.levelArena) {
                if (level == null) {
                    continue;
                }

                string key = level.name.Replace("Level - ", string.Empty);
                if (!arenas.ContainsKey(key)) {
                    arenas.Add(key, level);
                }
            }

            foreach (Level level in RunManager.instance.levelShop) {
                if (level == null) {
                    continue;
                }

                string key = level.name.Replace("Level - ", string.Empty);
                if (!shops.ContainsKey(key)) {
                    shops.Add(key, level);
                }
            }
        }

        public static SortedDictionary<string, Level> getMaps() {
            return maps;
        }

        public static SortedDictionary<string, Level> getArenas() {
            return arenas;
        }

        public static SortedDictionary<string, Level> getShops() {
            return shops;
        }

        public static Dictionary<Level, bool> getMapToggles() {
            return mapToggles;
        }

        public static void setMapEnabled(Level level, bool enabled) {
            if (level == null) {
                return;
            }

            if (mapToggles.ContainsKey(level)) {
                mapToggles[level] = enabled;
            }
        }

        public static bool isMapEnabled(Level level) {
            return level != null && mapToggles.GetValueOrDefault(level, false);
        }

        public static int getEnabledCount() {
            int count = 0;
            foreach (bool enabled in mapToggles.Values) {
                if (enabled) {
                    count++;
                }
            }
            return count;
        }

        public static void changeLevel(Level level) {
            if (level == null) {
                return;
            }

            nextLevel = level;
            RunManager.instance.ChangeLevel(true, false, RunManager.ChangeLevelType.RunLevel);
        }

        public static Level getNextLevel() {
            return nextLevel;
        }

        public static void clearNextLevel() {
            nextLevel = null;
        }

        public static void resetLevels() {
            if (RunManager.instance == null) {
                return;
            }

            foreach (KeyValuePair<Level, bool> entry in new List<KeyValuePair<Level, bool>>(mapToggles)) {
                Level level = entry.Key;
                if (level == null) {
                    continue;
                }

                if (!entry.Value) {
                    if (RunManager.instance.levels.Contains(level)) {
                        RunManager.instance.levels.Remove(level);
                    }
                } else {
                    if (!RunManager.instance.levels.Contains(level)) {
                        RunManager.instance.levels.Add(level);
                    }
                }
            }
        }
    }
}
