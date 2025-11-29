using MS_ZEsettings.Preferences;
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
        public Shake(
            IModSharp modSharp,
            IClientManager clients,
            IHookManager hooks,
            IEntityManager entities,
            Prefs prefs)
        {
            _modSharp = modSharp;
            _clients = clients;
            _hooks = hooks;
            _entities = entities;
            _prefs = prefs;
        }

        private readonly bool[] _noShakeFlags = new bool[PlayerSlot.MaxPlayerSlot];
        private readonly IModSharp _modSharp;
        private readonly IClientManager _clients;
        private readonly IHookManager _hooks;
        private readonly IEntityManager _entities;
        private readonly Prefs _prefs;


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
        public ECommandAction OnShakeCommand(IGameClient client, StringCommand command)
        {
            bool current = _prefs.GetCachedPreference(client.SteamId, "NoShake");
            bool next = !current;

            // 更新偏好 (寫入 cookie)
            _prefs.SetPreference(client, "NoShake", next);

            // 更新快取
            _prefs.UpdateCachedPreference(client.SteamId, "NoShake", next);

            // 顯示提示
            client.GetPlayerController()?.Print(
                command.ChatTrigger ? HudPrintChannel.Chat : HudPrintChannel.Console,
                $"[NoShake] {(next ? "Enabled" : "Disabled")}"
            );

            return ECommandAction.Stopped;
        }
        public void ApplyShake(IGameClient client)
        {
            if (!client.IsValid)
                return;

            bool disabled = _prefs.GetCachedPreference(client.SteamId, "NoShake");

            // 更新快取陣列
            _noShakeFlags[client.Slot] = disabled;

            // 顯示提示
            client.GetPlayerController()?.Print(
                HudPrintChannel.Chat,
                $"[NoShake] {(disabled ? "Enabled" : "Disabled")}"
            );
        }

    }
}

