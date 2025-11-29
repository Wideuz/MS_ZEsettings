using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace MS_ZEsettings.Listener
{
    public sealed class ZEClientListener : IClientListener
    {
        private readonly ITransmitManager _transmitManager;
        private readonly ILogger<ZEClientListener> _logger;
        private readonly ISharedSystem _sharedSystem;
        private readonly IModSharp _modsharp;

        public ZEClientListener(ITransmitManager transmitManager, ILogger<ZEClientListener> logger,
            ISharedSystem sharedSystem, IModSharp modsharp)
        {
            _transmitManager = transmitManager;
            _logger = logger;
            _sharedSystem = sharedSystem;
            _modsharp = modsharp;
        }

        public int ListenerVersion => IClientListener.ApiVersion;
        public int ListenerPriority => 0;

        public bool OnClientPreAdminCheck(IGameClient client) => false;

        public void OnClientConnected(IGameClient client)
        {
            
        }

        public void OnClientPutInServer(IGameClient client)
        {
            RecipientFilter filter = new RecipientFilter(client);
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, $" {ChatColor.Red}[ZEClient] {ChatColor.White} Hello", filter);
        }

        public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
        {
            
        }

        public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
        {
            
        }
        public void OnClientSettingChanged(IGameClient client)
        {
            
        }
    }
}
