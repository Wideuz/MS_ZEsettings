using Microsoft.Extensions.Logging;
using MS_StopMiscSound;
using MS_ZEsettings.Commands;
using MS_ZEsettings.Preferences;
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

        private readonly Prefs _prefs;

        public ZEClientListener(ITransmitManager transmitManager, ILogger<ZEClientListener> logger,
            ISharedSystem sharedSystem, IModSharp modsharp, Prefs prefs,
            StopSound stopSound,
            Shake shake,
            StopMiscSound weaponSound)
        {
            _transmitManager = transmitManager;
            _logger = logger;
            _sharedSystem = sharedSystem;
            _modsharp = modsharp;
            _prefs = prefs;
        }


        public int ListenerVersion => IClientListener.ApiVersion;
        public int ListenerPriority => 0;

        public bool OnClientPreAdminCheck(IGameClient client) => false;

        public void OnClientConnected(IGameClient client)
        {
          
        }

        public void OnClientPutInServer(IGameClient client)
        {
            if (!client.IsValid || client.IsFakeClient)
                return;

            // 預設值先放 false，等 OnCookieLoad 再更新
            _prefs.InitializeClient(client.SteamId);
        }

        public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
        {
            
        }

        public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
        {
            _prefs.CleanupClient(client.SteamId);
        }
        public void OnClientSettingChanged(IGameClient client)
        {
            
        }
    }
}
