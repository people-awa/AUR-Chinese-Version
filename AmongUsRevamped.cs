global using HarmonyLib;
global using System.Collections.Generic;
global using System.Linq;
global using Object = UnityEngine.Object;

using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System;
using UnityEngine;

namespace AmongUsRevamped;

// dotnet build -c release
// dotnet build -c android

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static BepInEx.Logging.ManualLogSource Logger;
    public static BasePlugin Instance;

    // 预设配置
    public static ConfigEntry<string> 预设1 { get; private set; }
    public static ConfigEntry<string> 预设2 { get; private set; }
    public static ConfigEntry<string> 预设3 { get; private set; }
    public static ConfigEntry<string> 预设4 { get; private set; }
    public static ConfigEntry<string> 预设5 { get; private set; }

    // 客户端选项
    public static ConfigEntry<bool> 游戏大师 { get; private set; }
    public static ConfigEntry<bool> 解锁FPS { get; private set; }
    public static ConfigEntry<bool> 显示FPS { get; private set; }
    public static ConfigEntry<bool> 自动开始 { get; private set; }
    public static ConfigEntry<bool> 深色主题 { get; private set; }
    public static ConfigEntry<bool> 大厅音乐 { get; private set; }

    public static NormalGameOptionsV10 普通模式选项 =>
        GameOptionsManager.Instance != null
            ? GameOptionsManager.Instance.currentNormalGameOptions
            : null;

    public static HideNSeekGameOptionsV10 捉迷藏模式选项 =>
        GameOptionsManager.Instance != null
            ? GameOptionsManager.Instance.currentHideNSeekGameOptions
            : null;

    public static MapNames 当前地图 => (MapNames)普通模式选项.MapId;

    public static bool 是否发生参数异常;
    public static string 凭据文本;
    public const string 模组版本 = "v1.7.0";

    public static float 游戏计时器;

    public static readonly Dictionary<int, int> 发言开始时间 = [];

    public static PlayerControl[] 所有玩家控制器
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            var i = 0;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId >= 254) continue;

                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }

    public override void Load()
    {
        var handler = AmongUsRevamped.Logger.Handler("GitVersion");

        Logger = BepInEx.Logging.Logger.CreateLogSource("AmongUsRevamped");
        AmongUsRevamped.Logger.Enable();
        Instance = this;

        // 预设名称配置
        预设1 = Config.Bind("预设名称选项", "Preset1", "预设 1");
        预设2 = Config.Bind("预设名称选项", "Preset2", "预设 2");
        预设3 = Config.Bind("预设名称选项", "Preset3", "预设 3");
        预设4 = Config.Bind("预设名称选项", "Preset4", "预设 4");
        预设5 = Config.Bind("预设名称选项", "Preset5", "预设 5");

        // 客户端选项配置
        自动开始 = Config.Bind("客户端选项", "自动开始", false);
        游戏大师 = Config.Bind("客户端选项", "游戏大师", false);
        解锁FPS = Config.Bind("客户端选项", "解锁 FPS", false);
        显示FPS = Config.Bind("客户端选项", "显示 FPS", false);
        自动开始 = Config.Bind("客户端选项", "自动开始", false);
        深色主题 = Config.Bind("客户端选项", "深色主题", false);
        大厅音乐 = Config.Bind("客户端选项", "大厅音乐", false);

        Translator.Init();
        BanManager.Init();

        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    class 模组管理器晚更新补丁
    {
        public static void Prefix(ModManager __instance)
        {
            __instance.ShowModStamp();
            LateTask.Update(Time.deltaTime);
        }
    }

    public enum 颜色名称
    {
        红色 = 0,
        蓝色 = 1,
        绿色 = 2,
        粉色 = 3,
        橙色 = 4,
        黄色 = 5,
        黑色 = 6,
        白色 = 7,
        紫色 = 8,
        棕色 = 9,
        青色 = 10,
        酸橙色 = 11,
        栗色 = 12,
        玫瑰色 = 13,
        香蕉色 = 14,
        灰色 = 15,
        棕褐色 = 16,
        珊瑚色 = 17,
        深绿色 = 18
    }

    // Innersloth 现在会屏蔽超过 5 个数字的消息
    public static readonly char[] 圆圈数字 =
    {
        '⓪',
        '①',
        '②',
        '③',
        '④',
        '⑤',
        '⑥',
        '⑦',
        '⑧',
        '⑨'
    };
}