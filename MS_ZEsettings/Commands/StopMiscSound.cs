using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace MS_StopMiscSound
{
    public class StopMiscSound
    {
        private readonly IClientManager _clients;
        private readonly IHookManager _hooks;
        private readonly bool[] _weaponSounds = new bool[PlayerSlot.MaxPlayerSlot];
        private readonly bool[] _footSteps = new bool[PlayerSlot.MaxPlayerSlot];

        readonly string[] FootStepsArray = [
            "T_Default.StepLeft",
            "CT_Default.StepLeft"
        ];

        readonly string[] WeaponSoundsArray = [
            "Weapon_Knife.HitWall",
            "Weapon_Knife.Slash",
            "Weapon_Knife.Hit",
            "Weapon_Knife.Stab",
            "Weapon_sg556.ZoomIn",
            "Weapon_sg556.ZoomOut",
            "Weapon_AUG.ZoomIn",
            "Weapon_AUG.ZoomOut",
            "Weapon_SSG08.Zoom",
            "Weapon_SSG08.ZoomOut",
            "Weapon_SCAR20.Zoom",
            "Weapon_SCAR20.ZoomOut",
            "Weapon_G3SG1.Zoom",
            "Weapon_G3SG1.ZoomOut",
            "Weapon_AWP.Zoom",
            "Weapon_AWP.ZoomOut",
            "Weapon_Revolver.Prepare",
            "Weapon.AutoSemiAutoSwitch"
        ];

        public StopMiscSound(IClientManager clients, IHookManager hooks)
        {
            _clients = clients;
            _hooks = hooks;
        }

        public void Init()
        {
            _hooks.EmitSound.InstallHookPre(OnEmitSound);
            _clients.InstallCommandCallback("loud", OnWeaponCommand);
            _clients.InstallCommandCallback("footsteps", OnFootStepCommand);
        }

        public void Shutdown()
        {
            _hooks.EmitSound.RemoveHookPre(OnEmitSound);
            _clients.RemoveCommandCallback("loud", OnWeaponCommand);
            _clients.RemoveCommandCallback("footsteps", OnFootStepCommand);
        }

        private HookReturnValue<SoundOpEventGuid> OnEmitSound(IEmitSoundHookParams param, HookReturnValue<SoundOpEventGuid> previousResult)
        {
            if (previousResult.Action is EHookAction.SkipCallReturnOverride) return default;

            // Footsteps
            for (int i = 0; i < FootStepsArray.Length; i++)
            {
                if (param.SoundName.Equals(FootStepsArray[i]))
                {
                    param.UpdateReceiver(new NetworkReceiver(
                        [.. _clients.GetGameClients(true).Where(x => !_footSteps[x.Slot]).Select(x => x.Slot)]
                    ));
                    return new HookReturnValue<SoundOpEventGuid>(EHookAction.ChangeParamReturnDefault);
                }
            }

            // Weapon sounds
            for (int i = 0; i < WeaponSoundsArray.Length; i++)
            {
                if (param.SoundName.Equals(WeaponSoundsArray[i]))
                {
                    param.UpdateReceiver(new NetworkReceiver(
                        [.. _clients.GetGameClients(true).Where(x => !_weaponSounds[x.Slot]).Select(x => x.Slot)]
                    ));
                    return new HookReturnValue<SoundOpEventGuid>(EHookAction.ChangeParamReturnDefault);
                }
            }

            return new HookReturnValue<SoundOpEventGuid>();
        }

        private ECommandAction OnWeaponCommand(IGameClient client, StringCommand command)
        {
            if (client == null || !client.IsValid) return ECommandAction.Stopped;

            _weaponSounds[client.Slot] = !_weaponSounds[client.Slot];

            client.GetPlayerController()?.Print(
                command.ChatTrigger ? HudPrintChannel.Chat : HudPrintChannel.Console,
                $"[StopMiscSound] Weapon sounds {(_weaponSounds[client.Slot] ? "Disabled" : "Enabled")}"
            );

            return ECommandAction.Stopped;
        }

        private ECommandAction OnFootStepCommand(IGameClient client, StringCommand command)
        {
            if (client == null || !client.IsValid) return ECommandAction.Stopped;

            _footSteps[client.Slot] = !_footSteps[client.Slot];

            client.GetPlayerController()?.Print(
                command.ChatTrigger ? HudPrintChannel.Chat : HudPrintChannel.Console,
                $"[StopMiscSound] Footsteps {(_footSteps[client.Slot] ? "Disabled" : "Enabled")}"
            );

            return ECommandAction.Stopped;
        }
    }
}

