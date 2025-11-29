using MS_ZEsettings.Preferences;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace MS_ZEsettings.Commands
{
    public class StopSound
    {
        private readonly IClientManager _clients;
        private readonly ITransmitManager _transmits;
        private readonly IModSharp _modsharp;
        private readonly Prefs _prefs;

        // 玩家狀態 (是否停用音效)
        private readonly Dictionary<SteamID, bool> _stopSoundPrefs = new();

        public StopSound(IClientManager clients, ITransmitManager transmits, IModSharp modSharp, Prefs prefs)
        {
            _clients = clients;
            _transmits = transmits;
            _modsharp = modSharp;
            _prefs = prefs;
        }

        // 初始化
        public void Init()
        {
            _clients.InstallCommandCallback("stopsound", OnStopSoundCommand);
        }

        // 清理
        public void Shutdown()
        {
            _clients.RemoveCommandCallback("stopsound", OnStopSoundCommand);
        }

        // 指令邏輯
        public ECommandAction OnStopSoundCommand(IGameClient client, StringCommand command)
        {
            if (!client.IsValid)
                return ECommandAction.Stopped;

            RecipientFilter filter = new RecipientFilter(client);

            // 直接用 SteamId 讀取並翻轉
            bool current = _stopSoundPrefs.TryGetValue(client.SteamId, out var value) && value;
            bool next = !current;

            _stopSoundPrefs[client.SteamId] = next;
            _prefs.SetPreference(client, "StopSound", next);

            string status = next ? "Mute" : "Unmute";
            _transmits.SetTempEntState(BlockTempEntType.FireBullets, client.Slot, next);
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat,
                $" {ChatColor.Red}[StopSound:Gun] {ChatColor.White}: You {status} sound", filter);

            return ECommandAction.Stopped;
        }

        public void ApplyStopSound(IGameClient client)
        {
            if (!client.IsValid)
                return;

            // 從偏好讀取 StopSound 狀態，預設 false
            bool enabled = _prefs.GetPreference(client, "StopSound", false);

            if (enabled)
            {
                RecipientFilter filter = new RecipientFilter(client);

                // 更新傳輸狀態 (阻止 FireBullets TempEnt)
                _transmits.SetTempEntState(BlockTempEntType.FireBullets, client.Slot, true);

                // 顯示提示訊息
                _modsharp.PrintChannelFilter(
                    HudPrintChannel.Chat,
                    $" {ChatColor.Red}[StopSound:Gun] {ChatColor.White}: You Mute sound",
                    filter
                );
            }
        }
    }
}
