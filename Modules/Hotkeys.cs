using AmongUs.Data;
using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

#if !ANDROID
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class Hotkeys
{
    public static int IncrementMultiplier;

    public static void Postfix()
    {
        if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null)
            return;

        bool Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool Ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool Enter = Input.GetKeyDown(KeyCode.Return);

        // ================= 断网/单机移动控制 =================
        if ((!AmongUsClient.Instance.IsGameStarted || !Utils.IsOnlineGame) &&
            Utils.CanMove &&
            PlayerControl.LocalPlayer.Collider != null)
        {
            PlayerControl.LocalPlayer.Collider.enabled = !Ctrl;
        }

        if (!AmongUsClient.Instance.AmHost) return;

        // ================= 强制结束游戏（快捷键） =================
        if (Input.GetKey(KeyCode.L) && Shift && Enter)
        {
            MessageWriter writer = AmongUsClient.Instance.StartEndGame();
            writer.Write((byte)GameOverReason.ImpostorDisconnect);
            AmongUsClient.Instance.FinishEndGame(writer);
        }

        // ================= 强制关闭会议 =================
        if (Input.GetKey(KeyCode.M) && Shift && Enter && Utils.InGame)
        {
            if (Utils.IsMeeting)
            {
                MeetingHud.Instance.RpcClose();
            }
        }

        // ================= 跳过开局倒计时 =================
        if (Shift &&
            GameStartManager.InstanceExists &&
            GameStartManager.Instance != null &&
            GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown &&
            !HudManager.Instance.Chat.IsOpenOrOpening)
        {
            GameStartManager.Instance.countDownTimer = 0;
        }

        // ================= 取消开局倒计时 =================
        if (Input.GetKeyDown(KeyCode.C) &&
            GameStartManager.InstanceExists &&
            GameStartManager.Instance != null &&
            GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown &&
            !HudManager.Instance.Chat.IsOpenOrOpening &&
            Utils.IsLobby)
        {
            Logger.Info("已重置开局倒计时", "快捷键系统");
            GameStartManager.Instance.ResetStartState();
            Logger.SendInGame("已取消开局倒计时");
        }

        // ================= 输入增量倍率 =================
        if (Shift)
            IncrementMultiplier = 5;
        else if (Ctrl)
            IncrementMultiplier = 10;
        else
            IncrementMultiplier = 1;
    }
}
#endif