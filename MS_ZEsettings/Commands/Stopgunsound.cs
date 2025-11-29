using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Definition;
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

        // 玩家狀態 (是否停用音效)
        private readonly bool[] _stopSoundFlags = new bool[PlayerSlot.MaxPlayerSlot];

        public StopSound(IClientManager clients, ITransmitManager transmits, IModSharp modSharp)
        {
            _clients = clients;
            _transmits = transmits;
            _modsharp = modSharp;
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
            if (client == null || !client.IsValid)
                return ECommandAction.Stopped;
            RecipientFilter filter = new RecipientFilter(client);

            // 切換狀態
            _stopSoundFlags[client.Slot] = !_stopSoundFlags[client.Slot];

            string status = _stopSoundFlags[client.Slot] ? "ON" : "OFF";
            // 更新傳輸狀態 (這裡只示範 FireBullets，可自行擴充)
            _transmits.SetTempEntState(BlockTempEntType.FireBullets, client.Slot, !_stopSoundFlags[client.Slot]);
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat ,$" {ChatColor.Red}[StopSound:Gun] {ChatColor.White}: You are {status} sound", filter);

            return ECommandAction.Stopped;
        }
    }
}
