using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Listeners;
using System;
using System.Collections.Generic;
using System.Text;

namespace MS_ZEsettings.Listener
{
    public sealed class ZEGamelistener : IGameListener
    {
        private readonly ILogger<ZEGamelistener> _logger;
        private readonly ISharedSystem _sharedSystem;

        public ZEGamelistener(ISharedSystem sharedSystem, ILogger<ZEGamelistener> logger)
        {
            _sharedSystem = sharedSystem;
            _logger = logger;
        }

        int IGameListener.ListenerVersion => IGameListener.ApiVersion;
        int IGameListener.ListenerPriority => 0;

        public void OnResourcePrecache()
        {
            
            
        }

        public void OnRoundRestarted()
        {
            
            
        }


        public void OnServerInit()
        {
            _logger.LogInformation($" [ZEGame] : Working");
        }
        
    }
}
