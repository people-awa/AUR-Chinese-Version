using Hazel;
using System;
using UnityEngine;

namespace AmongUsRevamped;

internal class EACR
{
    public static bool PlayerControlReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (pc == null || reader == null) return false;

        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;

            switch (rpc)
            {
                case RpcCalls.StartMeeting:
                {
                    AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                    Logger.SendInGame($"{pc.Data.PlayerName} 因尝试非法开会被封禁（疑似作弊）");
                    Logger.Info($"{pc.Data.PlayerName} 因非法开会被封禁（作弊行为）", "EACR");
                    return true;
                }
            }

            switch (callId)
            {
                case 101: // Aum Chat
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} 因 AUM 聊天异常调用被封禁（作弊）");
                            Logger.Info($"{pc.Data.PlayerName} AUM Chat 异常 RPC（作弊）", "EACR");
                            return true;
                        }
                    }
                    catch { }
                    break;

                case unchecked((byte)42069):
                    try
                    {
                        var aumid = sr.ReadByte();

                        if (aumid == pc.PlayerId)
                        {
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} 因 AUM RPC 被封禁（作弊）");
                            Logger.Info($"{pc.Data.PlayerName} AUM RPC 检测作弊", "EACR");
                            return true;
                        }
                    }
                    catch { }
                    break;

                case 119: // KN Chat
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} 因 KN 聊天异常被封禁（作弊）");
                            Logger.Info($"{pc.Data.PlayerName} KN Chat 异常 RPC", "EACR");
                            return true;
                        }
                    }
                    catch { }
                    break;

                case 250: // KN
                    if (sr.BytesRemaining == 0)
                    {
                        AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                        Logger.SendInGame($"{pc.Data.PlayerName} 因 KN RPC 被封禁（作弊）");
                        Logger.Info($"{pc.Data.PlayerName} KN RPC 空数据异常", "EACR");
                        return true;
                    }
                    break;

                case unchecked((byte)420): // SickoMenu
                    if (sr.BytesRemaining == 0)
                    {
                        AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                        Logger.SendInGame($"{pc.Data.PlayerName} 使用 SickoMenu RPC 被封禁（作弊）");
                        Logger.Info($"{pc.Data.PlayerName} SickoMenu RPC 检测", "EACR");
                        return true;
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.Exception(e, "EACR");
        }
        return false;
    }

    public static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, byte amount)
    {
        var Mapid = Utils.GetActiveMapId();

        if (!AmongUsClient.Instance.AmHost)
            return false;

        if (player == null)
        {
            Logger.Warn("玩家对象为空", "EACR-RpcUpdateSystemCheck");
            return true;
        }

        if (systemType == SystemTypes.Sabotage)
        {
            if (!player.Data.Role.IsImpostor && !player.isNew)
            {
                AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
                Logger.SendInGame($"{player.Data.PlayerName} 因非法破坏任务被封禁（作弊）");
                Logger.Info($"{player.Data.PlayerName} 非内鬼触发破坏系统", "EACR");
            }
        }
        else if (systemType == SystemTypes.LifeSupp)
        {
            if (Mapid != 0 && Mapid != 1 && Mapid != 3) goto YesCheat;
            else if (amount != 64 && amount != 65) goto YesCheat;
        }
        else if (systemType == SystemTypes.Comms)
        {
            if (amount == 0)
            {
                if (Mapid == 1 || Mapid == 5) goto YesCheat;
            }
            else if (amount == 64 || amount == 65 || amount == 32 || amount == 33 || amount == 16 || amount == 17)
            {
                if (!(Mapid == 1 || Mapid == 5)) goto YesCheat;
            }
            else goto YesCheat;
        }
        else if (systemType == SystemTypes.Electrical)
        {
            if (Mapid == 5) goto YesCheat;
            if (amount >= 5) goto YesCheat;
        }
        else if (systemType == SystemTypes.Laboratory)
        {
            if (Mapid != 2) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.Reactor)
        {
            if (Mapid == 2 || Mapid == 4) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.HeliSabotage)
        {
            if (Mapid != 4) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 16 || amount == 17 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.MushroomMixupSabotage)
        {
            goto YesCheat;
        }

        if (Utils.IsMeeting && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating)
        {
            Logger.SendInGame($"{player.Data.PlayerName} 可能触发非法破坏（疑似作弊）");
            Logger.Info($"{player.Data.PlayerName} 可疑破坏行为检测", "EACR");
            return true;
        }

        return false;

    YesCheat:
        {
            AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
            Logger.SendInGame($"{player.Data.PlayerName} 因非法破坏系统被封禁（作弊）");
            Logger.Info($"{player.Data.PlayerName} 非法破坏系统检测", "EACR");
            return true;
        }
    }
}