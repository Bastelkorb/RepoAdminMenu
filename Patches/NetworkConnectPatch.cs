using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace RepoAdminMenu.Utils {
    internal class NetworkUtil : IOnEventCallback {

        internal static NetworkUtil instance { get; private set; } = null;

        internal bool registered = false;
        internal string clientRandomString = string.Empty;

        private static readonly System.Random random = new System.Random();

        internal const byte RAMEventCode = 180;

        public NetworkUtil() {
            instance = this;
            if (!registered) {
                RepoAdminMenu.mls.LogInfo("Registering network event callbacks");
                PhotonNetwork.AddCallbackTarget(this);
                registered = true;
            }
        }

        public static void Init() {
            if (instance == null) {
                new NetworkUtil();
            }
        }

        private static string randomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal static void SendCommandSteamIDString(string command, string steamId, string value, ReceiverGroup receiverGroup) {
            if (PhotonNetwork.NetworkClientState != ClientState.Joined) {
                return;
            }

            PhotonNetwork.RaiseEvent(RAMEventCode, new object[] { command, steamId, value }, new RaiseEventOptions { Receivers = receiverGroup }, SendOptions.SendReliable);
        }

        internal static void SendCommandSteamIDStringInt(string command, string steamId, string str, int num, ReceiverGroup receiverGroup) {
            if (PhotonNetwork.NetworkClientState != ClientState.Joined) {
                return;
            }

            PhotonNetwork.RaiseEvent(RAMEventCode, new object[] { command, steamId, str, num }, new RaiseEventOptions { Receivers = receiverGroup }, SendOptions.SendReliable);
        }

        internal static void SendCommandString(string command, string value, ReceiverGroup receiverGroup) {
            if (PhotonNetwork.NetworkClientState != ClientState.Joined) {
                return;
            }

            PhotonNetwork.RaiseEvent(RAMEventCode, new object[] { command, value }, new RaiseEventOptions { Receivers = receiverGroup }, SendOptions.SendReliable);
        }

        internal static void SendCommand(string command, ReceiverGroup receiverGroup) {
            if (PhotonNetwork.NetworkClientState != ClientState.Joined) {
                return;
            }

            PhotonNetwork.RaiseEvent(RAMEventCode, new object[] { command }, new RaiseEventOptions { Receivers = receiverGroup }, SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent) {
            if (photonEvent.Code != RAMEventCode) {
                return;
            }

            if (photonEvent.CustomData is not object[] args) {
                RepoAdminMenu.mls.LogWarning("Received network event without array payload.");
                return;
            }

            if (args.Length == 1) {
                string cmd = args[0]?.ToString();
                switch (cmd) {
                    case "NeedSettings":
                        Settings.UpdateClients();
                        break;
                    default:
                        RepoAdminMenu.mls.LogWarning("Network event: Unknown command(" + args.Length + "): " + cmd);
                        break;
                }
                return;
            }

            if (args.Length == 2) {
                string cmd = args[0]?.ToString();
                string payload = args[1]?.ToString();

                switch (cmd) {
                    case "SettingsSync":
                        if (!SemiFunc.IsMasterClient()) {
                            RepoAdminMenu.mls.LogInfo("Syncing settings with Master Client");
                            Settings.loadFromJson(payload);
                        }
                        break;
                    default:
                        RepoAdminMenu.mls.LogWarning("Network event: Unknown command(" + args.Length + "): " + cmd);
                        break;
                }
                return;
            }

            if (args.Length == 3) {
                string cmd = args[0]?.ToString();
                string steamId = args[1]?.ToString();

                if (string.IsNullOrEmpty(steamId)) {
                    RepoAdminMenu.mls.LogWarning("Network event: Missing steamId in command '" + cmd + "'.");
                    return;
                }

                PlayerAvatar player = SemiFunc.PlayerGetFromSteamID(steamId);
                if (player == null) {
                    RepoAdminMenu.mls.LogWarning("Network event: Invalid player: " + steamId);
                    return;
                }

                switch (cmd) {
                    default:
                        RepoAdminMenu.mls.LogWarning("Unknown command(" + args.Length + "): " + cmd);
                        break;
                }
                return;
            }

            if (args.Length == 4) {
                string cmd = args[0]?.ToString();
                string steamId = args[1]?.ToString();

                if (string.IsNullOrEmpty(steamId)) {
                    RepoAdminMenu.mls.LogWarning("Network event: Missing steamId in command '" + cmd + "'.");
                    return;
                }

                PlayerAvatar player = SemiFunc.PlayerGetFromSteamID(steamId);
                if (player == null) {
                    RepoAdminMenu.mls.LogWarning("Network event: Invalid player: " + steamId);
                    return;
                }

                switch (cmd) {
                    case "UpgradeSync":
                        UpgradeUtil.UpgradeSync(player, args[2]?.ToString(), (int)args[3]);
                        break;
                    default:
                        RepoAdminMenu.mls.LogWarning("Unknown command(" + args.Length + "): " + cmd);
                        break;
                }
                return;
            }

            RepoAdminMenu.mls.LogWarning("Received network event with incorrect argument count: " + args.Length);
        }
    }
}
