using InnerNet;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;

// https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Modules/BanManager.cs
namespace AmongUsRevamped;

public static class BanManager
{
    public static readonly string DataPath =
#if ANDROID
        Application.persistentDataPath;
#else
        ".";
#endif

    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", "");
    private static readonly string DenyNameListPath = $"{DataPath}/AUR-DATA/DenyNameList.txt";
    private static string BanListPath = $"{DataPath}/AUR-DATA/Banlist.txt";
    private static string BanWordPath = $"{DataPath}/AUR-DATA/Bannedwords.txt";
    private static string VipListPath = $"{DataPath}/AUR-DATA/VIP.txt";
    private static string ModeratorListPath = $"{DataPath}/AUR-DATA/Moderator.txt";
    private static string AdminListPath = $"{DataPath}/AUR-DATA/Admin.txt";

    public static List<string> TempBanWhiteList = [];

    public static void Init()
    {
        try
        {
            if (!Directory.Exists($"{DataPath}/AUR-DATA")) Directory.CreateDirectory($"{DataPath}/AUR-DATA");

            if (!File.Exists(DenyNameListPath))
            {
                Logger.Warn("正在创建 DenyNameList.txt 文件", "BanManager");
                File.Create(DenyNameListPath).Close();
            }
            if (!File.Exists(BanListPath))
            {
                Logger.Warn("正在创建 Banlist.txt 文件", "BanManager");
                File.Create(BanListPath).Close();
            }
            if (!File.Exists(BanWordPath))
            {
                Logger.Warn("正在创建 Banlist.txt 文件", "BanManager");
                File.Create(BanWordPath).Close();
            }
            if (!File.Exists(VipListPath))
            {
                Logger.Warn("正在创建 VIP.txt 文件", "BanManager");
                File.Create(VipListPath).Close();
            }
            if (!File.Exists(ModeratorListPath))
            {
                Logger.Warn("正在创建 Moderator.txt 文件", "BanManager");
                File.Create(ModeratorListPath).Close();
            }
            if (!File.Exists(AdminListPath))
            {
                Logger.Warn("正在创建 Admin.txt 文件", "BanManager");
                File.Create(AdminListPath).Close();
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "BanManager");
        }
    }

    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return "";
        string puid = player.ProductUserId;
        return GetHashedPuid(puid);
    }

    public static string GetHashedPuid(string puid)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();

        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }

    public static void AddBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || player == null) return;

        if (!CheckBanList(player?.FriendCode, player?.GetHashedPuid())
            && !TempBanWhiteList.Contains(player?.GetHashedPuid()))
        {
            if (player?.GetHashedPuid() != "" &&
                player?.GetHashedPuid() != null &&
                player?.GetHashedPuid() != "e3b0cb855")
            {
                var additionalInfo = "";

                File.AppendAllText(
                    BanListPath,
                    $"{player?.FriendCode},{player?.GetHashedPuid()},{player.PlayerName.RemoveHtmlTags()}{additionalInfo}\n"
                );

                Logger.SendInGame($"已将 {player?.PlayerName.RemoveHtmlTags()}/{player?.FriendCode}/{player?.GetHashedPuid()} 加入封禁列表");
            }
            else
            {
                Logger.Info($"无法将玩家 {player?.PlayerName.RemoveHtmlTags()}/{player?.FriendCode}/{player?.GetHashedPuid()} 添加到封禁列表", "AddBanPlayer");
            }
        }
    }

    public static void CheckBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        string friendcode = player?.FriendCode;

        if (CheckBanList(friendcode, player?.GetHashedPuid()))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            Logger.Info($"{player.PlayerName} 已存在于封禁列表中，已被封禁", "BanListBan");
            return;
        }

        if (TempBanWhiteList.Contains(player?.GetHashedPuid()))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            Logger.Info($"{player.PlayerName} 位于临时封禁列表中", "TempBan");
            return;
        }
    }

    public static bool CheckBanList(string code, string hashedpuid = "")
    {
        bool OnlyCheckPuid = false;

        if (code == "" && hashedpuid != "") OnlyCheckPuid = true;
        else if (code == "") return false;

        string noDiscrim = "";

        if (code.Contains('#'))
        {
            int index = code.IndexOf('#');
            noDiscrim = code[..index];
        }

        try
        {
            if (!Directory.Exists($"{DataPath}/AUR-DATA"))
                Directory.CreateDirectory($"{DataPath}/AUR-DATA");

            if (!File.Exists(BanListPath))
                File.Create(BanListPath).Close();

            using StreamReader sr = new(BanListPath);
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;

                if (!OnlyCheckPuid)
                {
                    if (line.Contains(code)) return true;
                    if (!string.IsNullOrEmpty(noDiscrim) && !line.Contains('#') && line.Contains(noDiscrim))
                        return true;
                }

                if (line.Contains(hashedpuid)) return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "CheckBanList");
        }

        return false;
    }

    public static bool IsPlayerInDenyName(ClientData client, string name)
    {
        if (name == "" || !AmongUsClient.Instance.AmHost) return false;

        var deniedNames = File.ReadAllLines(DenyNameListPath);

        if (deniedNames
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Any(code => name.Contains(code, StringComparison.OrdinalIgnoreCase)))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);

            Logger.Info($"{name} 因名称命中 DenyNameList.txt 黑名单而被踢出", "Kick");
            Logger.SendInGame($"{name} 因名称命中黑名单而被踢出");

            return true;
        }

        return false;
    }

    public static bool IsWordBanned(PlayerControl player, string input)
    {
        if (input == "" ||
            !AmongUsClient.Instance.AmHost ||
            Utils.CheckAccessLevel(player.Data.FriendCode) > 0)
            return false;

        var bannedWords = File.ReadAllLines(BanWordPath);

        if (bannedWords
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Any(code => input.Contains(code, StringComparison.OrdinalIgnoreCase)))
        {
            AmongUsClient.Instance.KickPlayer(player.Data.ClientId, false);

            Logger.Info($"{player.Data.PlayerName} 因发送违禁词而被踢出", "Kick");
            Logger.SendInGame($"{player.Data.PlayerName} 因发送违禁词而被踢出");

            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Select))]
    class BanMenuSelectPatch
    {
        public static void Postfix(BanMenu __instance, int clientId)
        {
            ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
            if (recentClient == null) return;

            if (!BanManager.CheckBanList(recentClient?.FriendCode, recentClient?.GetHashedPuid()))
                __instance.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
        }
    }
}