using Hazel;
using System;
using System.Runtime.CompilerServices;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
public static class FixedUpdate
{
    public static void Postfix()
    {
        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            Main.GameTimer += Time.fixedDeltaTime;
        }

        GameObject n = GameObject.Find("NewRequestInactive");
        if (n != null)
        {
            n.SetActive(false);
        }

        GameObject nr = GameObject.Find("NewRequest");
        if (nr != null)
        {
            nr.SetActive(false);
        }

        if (!AmongUsClient.Instance.AmHost) return;
        DisableDevice.FixedUpdate();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    private static Dictionary<byte, string> LastColors = new();
    private static float t;
    private static GameObject settingsLabel;

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255 || !AmongUsClient.Instance.AmHost) return;

        t += Time.deltaTime;
        if (t < 0.2f) return;
        t = 0f;

        // ================= 大厅权限颜色显示 =================
        if (Utils.IsLobby)
        {
            int access = Utils.CheckAccessLevel(__instance.Data.FriendCode);

            string color = access switch
            {
                1 => "yellow",
                2 => "purple",
                3 => "red",
                _ => "white"
            };

            if (!LastColors.TryGetValue(__instance.PlayerId, out var lastColor) ||
                lastColor != color ||
                !__instance.cosmetics.nameText.text.Contains($"<color={color}>"))
            {
                __instance.cosmetics.nameText.text =
                    $"<color={color}>{__instance.Data.PlayerName}</color>";

                LastColors[__instance.PlayerId] = color;
            }
        }

        if (settingsLabel == null)
            settingsLabel = GameObject.Find("GameSettingsLabel");

        int gamemode = Options.Gamemode.GetValue();

        // ================= 游戏模式逻辑 =================
        switch (gamemode)
        {
            case 1: // 0击杀冷却
                if (Main.NormalOptions.KillCooldown != 0.01f)
                    Main.NormalOptions.KillCooldown = 0.01f;

                if (Options.NoKcdSettingsOverride.GetBool() && settingsLabel == null)
                {
                    Main.NormalOptions.EmergencyCooldown = 0;
                    Main.NormalOptions.TaskBarMode = 0;
                }
                break;

            case 2: // 追击/躲藏模式
                if (Main.NormalOptions.KillCooldown != 2.5f)
                    Main.NormalOptions.KillCooldown = 2.5f;

                if (Options.SNSSettingsOverride.GetBool() && settingsLabel == null)
                    Main.NormalOptions.TaskBarMode = 0;
                break;

            case 3: // 速通模式
                if (settingsLabel == null)
                    Main.NormalOptions.TaskBarMode = 0;
                break;

            case 0: // 默认模式重置
                if (Main.NormalOptions.KillCooldown <= 0.01f)
                    Main.NormalOptions.KillCooldown = 25f;
                break;
        }

        // ================= 等级检测踢人 =================
        if (__instance.Data.PlayerLevel != 0 &&
            __instance.Data.PlayerLevel < Options.KickLowLevelPlayer.GetInt() - 1 &&
            __instance.Data.ClientId != AmongUsClient.Instance.HostId)
        {
            if (!Options.TempBanLowLevelPlayer.GetBool())
            {
                AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, false);

                Logger.Info(
                    $"{__instance.Data.PlayerName} 因等级低于 {Options.KickLowLevelPlayer.GetInt()} 被踢出",
                    "KickLowLevelPlayer");

                Logger.SendInGame(
                    $"{__instance.Data.PlayerName} 因等级过低被踢出（阈值 {Options.KickLowLevelPlayer.GetInt()}）");
            }
            else
            {
                AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, true);

                Logger.Info(
                    $"{__instance.Data.PlayerName} 因等级过低被封禁",
                    "BanLowLevelPlayer");

                Logger.SendInGame(
                    $"{__instance.Data.PlayerName} 因等级过低被封禁（阈值 {Options.KickLowLevelPlayer.GetInt()}）");
            }
        }

        // ================= 游戏计时结束判定 =================
        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            // ================= 躲藏追击自动胜利 =================
            if (Options.Gamemode.GetValue() == 2 &&
                !Utils.isHideNSeek &&
                Options.CrewAutoWinsGameAfter.GetInt() != 0 &&
                !Options.NoGameEnd.GetBool())
            {
                if (Main.GameTimer > Options.CrewAutoWinsGameAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);

                    Logger.Info(
                        $"游戏超过 {Options.CrewAutoWinsGameAfter.GetInt()} 秒，船员胜利",
                        "SNSManager");

                    NormalGameEndChecker.LastWinReason =
                        $"船员胜利（时间到）\n\n内鬼名单：\n" +
                        string.Join("\n", NormalGameEndChecker.imps.Select(p => p.Data.PlayerName));
                }
            }

            // ================= 速通模式自动结束 =================
            if (Options.Gamemode.GetValue() == 3 &&
                !Utils.isHideNSeek &&
                Options.GameAutoEndsAfter.GetInt() != 0 &&
                !Options.NoGameEnd.GetBool())
            {
                if (Main.GameTimer > Options.GameAutoEndsAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.CustomWinnerEndGame(PlayerControl.LocalPlayer, 0);

                    Logger.Info(
                        $"游戏超过 {Options.GameAutoEndsAfter.GetInt()} 秒，无人胜利",
                        "SpeedrunManager");

                    NormalGameEndChecker.LastWinReason = "无人胜利（时间结束）";
                }
            }
        }
    }
}