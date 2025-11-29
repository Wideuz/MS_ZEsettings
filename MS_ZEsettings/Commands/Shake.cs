using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace MS_ZEsettings.Commands
{
    public class Shake
    {
        private readonly IModSharp _modSharp;
        private readonly IClientManager _clients;
        private readonly IHookManager _hooks;
        private readonly IEntityManager _entities;

        // 玩家狀態 (是否停用震動效果)
        private readonly bool[] _noShakeFlags = new bool[65];

        public Shake(IModSharp modSharp, IClientManager clients, IHookManager hooks, IEntityManager entities)
        {
            _modSharp = modSharp;
            _clients = clients;
            _hooks = hooks;
            _entities = entities;
        }

        // 初始化
        public void Init()
        {
            _hooks.PostEventAbstract.InstallHookPre(OnShakeMessage);
            _modSharp.HookNetMessage(ProtobufNetMessageType.UM_Shake);

            _clients.InstallCommandCallback("shake", OnShakeCommand);
            _clients.InstallCommandCallback("noshake", OnShakeCommand);
        }

        // 清理
        public void Shutdown()
        {
            _hooks.PostEventAbstract.RemoveHookPre(OnShakeMessage);
            _clients.RemoveCommandCallback("shake", OnShakeCommand);
            _clients.RemoveCommandCallback("noshake", OnShakeCommand);
        }

        // 攔截震動訊息
        private HookReturnValue<NetworkReceiver> OnShakeMessage(IPostEventAbstractHookParams param, HookReturnValue<NetworkReceiver> previousResult)
        {
            if (param.MsgId == ProtobufNetMessageType.UM_Shake)
            {
                var receivers = new NetworkReceiver(
                    [.. _entities.GetPlayerControllers(true)
                        .Where(x => x.GetPlayerPawn() is { IsAlive: true })
                        .Where(x => !_noShakeFlags[x.PlayerSlot])
                        .Select(x => x.PlayerSlot)]
                );

                return new HookReturnValue<NetworkReceiver>(EHookAction.ChangeParamReturnDefault, receivers);
            }

            return default;
        }

        // 指令邏輯
        private ECommandAction OnShakeCommand(IGameClient client, StringCommand command)
        {
            if (client == null || !client.IsValid)
                return ECommandAction.Stopped;

            // 切換狀態
            _noShakeFlags[client.Slot] = !_noShakeFlags[client.Slot];

            // 顯示提示 (你可以自行改成 HUD 或 Console)
            client.GetPlayerController()?.Print(
                command.ChatTrigger ? HudPrintChannel.Chat : HudPrintChannel.Console,
                $"[NoShake] {(_noShakeFlags[client.Slot] ? "Enabled" : "Disabled")}"
            );

            return ECommandAction.Stopped;
        }
    }
}

