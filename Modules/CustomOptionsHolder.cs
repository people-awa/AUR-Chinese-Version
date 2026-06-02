using System.Threading.Tasks;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionHolder.cs
namespace AmongUsRevamped
{
    public static class CL
    {
        public static Color32 Hex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 3)
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";

            if (hex.Length != 6)
                throw new System.ArgumentException("Hex 颜色必须为 3 或 6 位字符。");

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255);
        }
    }

    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(Load);
        }

        public const int PresetId = 0;

        private static readonly string[] presets =
        {
            Main.Preset1.Value, Main.Preset2.Value, Main.Preset3.Value,
            Main.Preset4.Value, Main.Preset5.Value
        };

        // 游戏模式（UI显示）
        public static readonly string[] gameModes =
        {
            "标准模式", "0击杀冷却", "躲猫猫模式", "速通模式"
        };

        // 权限等级（UI显示）
        public static readonly string[] accessLevels =
        {
            "所有玩家",
            "<color=yellow>VIP</color>及以上",
            "<color=purple>管理员</color>及以上",
            "<color=red>最高管理员</color>",
            "仅自己"
        };

        // ================= 系统 =================

        public static OptionItem Gamemode;

        public static OptionItem TabGroupMain;

        public static OptionItem KickLowLevelPlayer;
        public static OptionItem TempBanLowLevelPlayer;

        public static OptionItem KickInvalidFriendCodes;
        public static OptionItem TempBanInvalidFriendCodes;

        public static OptionItem AutoKickStart;
        public static OptionItem AutoKickStartAsBan;
        public static OptionItem AutoKickStartTimes;
        public static OptionItem AutoKickStartStrength;

        public static OptionItem TabGroupAutomation;

        public static OptionItem AutoSendGameInfo;
        public static OptionItem AutoRejoinLobby;
        public static OptionItem AutoStartTimer;
        public static OptionItem WaitAutoStart;
        public static OptionItem PlayerAutoStart;

        public static OptionItem TabGroupMisc;

        public static OptionItem StartCountdown;
        public static OptionItem ColorCommandLevel;
        public static OptionItem AllowFortegreen;
        public static OptionItem NoGameEnd;

        public static OptionItem TabGroupAccess;

        public static OptionItem SlashColorCmd;
        public static OptionItem SlashRolesAndGamemodeCmd;
        public static OptionItem SlashLastGameCmd;
        public static OptionItem SlashHelpAndAurCmd;
        public static OptionItem SlashKickCmd;
        public static OptionItem SlashBanCmd;
        public static OptionItem SlashEndMeetingCmd;
        public static OptionItem SlashStartAndEndGameCmd;

        // ================= 游戏玩法 =================

        public static OptionItem TabGroupSabotages;

        public static OptionItem DeadImpostorsCanSabotage;
        public static OptionItem DisableSabotage;
        public static OptionItem DisableReactor;
        public static OptionItem DisableOxygen;
        public static OptionItem DisableLights;
        public static OptionItem DisableComms;
        public static OptionItem DisableHeli;
        public static OptionItem DisableMushroomMixup;
        public static OptionItem DisableCloseDoor;

        public static OptionItem TabGroupGameplayGeneral;

        public static OptionItem DisableAnnoyingMeetingCalls;

        public static OptionItem ChangeDecontaminationTime;
        public static OptionItem DecontaminationTimeOnMiraHQ;
        public static OptionItem DecontaminationDoorOpenTimeOnMiraHQ;
        public static OptionItem DecontaminationTimeOnPolus;
        public static OptionItem DecontaminationDoorOpenTimeOnPolus;
        public static OptionItem DisableSporeTrigger;

        public static OptionItem DisableDevices;

        // ================= 任务 =================

        public static OptionItem TabGroupTasks;

        public static OptionItem OverrideTaskSettings;
        public static OptionItem AllPlayersSameTasks;
        public static OptionItem TaskPercentNeededToWin;

        // ================= Gamemode =================

        public static OptionItem TabGroupStandard;
        public static OptionItem ChatBeforeFirstMeeting;

        public static OptionItem TabGroupHNS;
        public static OptionItem NumSeekers;

        public static OptionItem TabGroup0Kcd;
        public static OptionItem NoKcdSettingsOverride;

        public static OptionItem TabGroupSNS;

        public static OptionItem SNSSettingsOverride;
        public static OptionItem SNSChatInGame;
        public static OptionItem CrewAutoWinsGameAfter;
        public static OptionItem CantKillTime;

        // ================= 角色 =================

        public static OptionItem TabGroupCrewmate;

        public static OptionItem MayorPerc;
        public static OptionItem MayorExtraVoteCount;
        public static OptionItem MayorVentToMeeting;

        public static OptionItem TabGroupNeutral;

        public static OptionItem JesterPerc;
        public static OptionItem JesterCanVent;

        public static OptionItem TabGroupImpostor;

        public static bool IsLoaded = false;

        // ================= 加载 =================

        public static void Load()
        {
            if (IsLoaded) return;

            _ = PresetOptionItem.Create(0, TabGroup.SystemSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true);

            Gamemode = StringOptionItem.Create(1, Translator.Get("gamemode"), gameModes, 0, TabGroup.SystemSettings, false)
                .SetColor(Color.green)
                .SetHeader(true);

            TabGroupMain = TextOptionItem.Create(60000, Translator.Get("tabGroupMain"), TabGroup.SystemSettings)
                .SetColor(Color.blue);

            KickLowLevelPlayer = IntegerOptionItem.Create(60050, Translator.Get("kickLowLevelPlayer"), new(0, 100, 1), 0, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Level);

            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, Translator.Get("tempBanLowLevelPlayer"), false, TabGroup.SystemSettings, false)
                .SetParent(KickLowLevelPlayer);

            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, Translator.Get("kickInvalidFriendCodes"), true, TabGroup.SystemSettings, false);

            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, Translator.Get("tempBanInvalidFriendCodes"), false, TabGroup.SystemSettings, false)
                .SetParent(KickInvalidFriendCodes);

            AutoKickStart = BooleanOptionItem.Create(60123, Translator.Get("autoKickStart"), false, TabGroup.SystemSettings, false);

            AutoKickStartAsBan = BooleanOptionItem.Create(60124, Translator.Get("autoKickStartAsBan"), false, TabGroup.SystemSettings, false)
                .SetParent(AutoKickStart);

            AutoKickStartTimes = IntegerOptionItem.Create(60125, Translator.Get("autoKickStartTimes"), new(1, 10, 1), 1, TabGroup.SystemSettings, false)
                .SetParent(AutoKickStart)
                .SetValueFormat(OptionFormat.Times);

            AutoKickStartStrength = BooleanOptionItem.Create(60126, Translator.Get("autoKickStartStrength"), false, TabGroup.SystemSettings, false);

            TabGroupAutomation = TextOptionItem.Create(60149, Translator.Get("tabGroupAutomation"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);

            AutoSendGameInfo = BooleanOptionItem.Create(60150, Translator.Get("autoSendGameInfo"), true, TabGroup.SystemSettings, false);
            AutoRejoinLobby = BooleanOptionItem.Create(60210, Translator.Get("autoRejoinLobby"), false, TabGroup.SystemSettings, false);

            AutoStartTimer = IntegerOptionItem.Create(64420, Translator.Get("autoStartTimer"), new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);

            WaitAutoStart = IntegerOptionItem.Create(64421, Translator.Get("waitAutoStart"), new(10, 600, 10), 300, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);

            PlayerAutoStart = IntegerOptionItem.Create(64422, Translator.Get("playerAutoStart"), new(1, 15, 1), 1, TabGroup.SystemSettings, false);

            TabGroupMisc = TextOptionItem.Create(60379, Translator.Get("tabGroupMisc"), TabGroup.SystemSettings)
                .SetColor(Color.green);

            StartCountdown = IntegerOptionItem.Create(60380, Translator.Get("startCountdown"), new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);

            AllowFortegreen = BooleanOptionItem.Create(60382, Translator.Get("allowFortegreen"), false, TabGroup.SystemSettings, false);

            NoGameEnd = BooleanOptionItem.Create(60383, Translator.Get("noGameEnd"), false, TabGroup.SystemSettings, false)
                .SetColor(Color.red);

            TabGroupAccess = TextOptionItem.Create(60400, Translator.Get("tabGroupAccess"), TabGroup.SystemSettings)
                .SetColor(Color.red);

            SlashColorCmd = StringOptionItem.Create(60401, Translator.Get("slashColorCmd"), accessLevels, 1, TabGroup.SystemSettings, false);
            SlashRolesAndGamemodeCmd = StringOptionItem.Create(60402, Translator.Get("slashRolesAndGamemodeCmd"), accessLevels, 1, TabGroup.SystemSettings, false);
            SlashLastGameCmd = StringOptionItem.Create(60403, Translator.Get("slashLastGameCmd"), accessLevels, 1, TabGroup.SystemSettings, false);
            SlashHelpAndAurCmd = StringOptionItem.Create(60404, Translator.Get("slashHelpAndAurCmd"), accessLevels, 1, TabGroup.SystemSettings, false);
            SlashKickCmd = StringOptionItem.Create(60405, Translator.Get("slashKickCmd"), accessLevels, 2, TabGroup.SystemSettings, false);
            SlashBanCmd = StringOptionItem.Create(60406, Translator.Get("slashBanCmd"), accessLevels, 2, TabGroup.SystemSettings, false);
            SlashEndMeetingCmd = StringOptionItem.Create(60407, Translator.Get("slashEndMeetingCmd"), accessLevels, 3, TabGroup.SystemSettings, false);
            SlashStartAndEndGameCmd = StringOptionItem.Create(60408, Translator.Get("slashStartAndEndGameCmd"), accessLevels, 3, TabGroup.SystemSettings, false);

            // 下面角色与模式部分同理（已汉化文本资源 Translator 控制）
        }
    }
}