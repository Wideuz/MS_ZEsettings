using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MS_StopMiscSound;
using MS_ZEsettings.Commands;
using MS_ZEsettings.Listener;
using MS_ZEsettings.Preferences;
using Sharp.Extensions.GameEventManager;
using Sharp.Shared;
using Sharp.Shared.Abstractions;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using System.Drawing;
using static Sharp.Shared.Managers.IClientManager;

namespace MS_ZEsettings
{
    public sealed class ZEPlayerFunctions : IModSharpModule 
    {
        public string DisplayName => "Leader";
        public string DisplayAuthor => "Widez";

        private readonly ILogger<ZEPlayerFunctions> _logger;
        private readonly ISharedSystem _sharedSystem;
        private readonly IClientManager _clientManager;
        private readonly ITransmitManager _transmitManager;
        private readonly IModSharp _modSharp;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHookManager _hookManager;
        private readonly string _dllPath;
        private readonly string _sharpPath;
        private ZEClientListener? _clientListener;
        private ZEGamelistener? _gameListener;
        private Prefs? _prefs;
        private StopSound? _stopSound;
        private Shake? _shake;
        private StopMiscSound? _weaponSound;

        public ZEPlayerFunctions(ISharedSystem sharedSystem,
            string? dllPath,
            string? sharpPath,
            Version? version,
            Microsoft.Extensions.Configuration.IConfiguration? coreConfiguration,
            bool hotReload = false)
        {
            ArgumentNullException.ThrowIfNull(dllPath);
            ArgumentNullException.ThrowIfNull(sharpPath);
            ArgumentNullException.ThrowIfNull(version);
            ArgumentNullException.ThrowIfNull(coreConfiguration);
            _dllPath = dllPath;
            _sharpPath = sharpPath;
            _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));

            var services = new ServiceCollection();
            services.AddSingleton(_sharedSystem.GetLoggerFactory());
            services.AddLogging();
            services.AddGameEventManager(_sharedSystem);
            _serviceProvider = services.BuildServiceProvider();
            _serviceProvider.LoadAllSharpExtensions();

            _logger = _sharedSystem.GetLoggerFactory().CreateLogger<ZEPlayerFunctions>();
            _clientManager = _sharedSystem.GetClientManager();
            _transmitManager = _sharedSystem.GetTransmitManager();
            _modSharp = _sharedSystem.GetModSharp();
            _hookManager = _sharedSystem.GetHookManager();

        }

        public bool Init()
        {
            _prefs = new Prefs(_sharedSystem,_modSharp);
            _prefs.Init();

            _stopSound = new StopSound(_clientManager, _transmitManager, _modSharp, _prefs);
            _shake = new Shake(_modSharp, _clientManager, _hookManager, _sharedSystem.GetEntityManager(), _prefs);
            _weaponSound = new StopMiscSound(_clientManager, _hookManager, _prefs);

            _prefs.PreferencesApplied += client =>
            {
                if (_prefs.GetCachedPreference(client.SteamId, "StopSound"))
                    _stopSound.ApplyStopSound(client);
                
            };

            _clientListener = new ZEClientListener(_transmitManager,
                _sharedSystem.GetLoggerFactory().CreateLogger<ZEClientListener>(),
                _sharedSystem, _modSharp, _prefs, _stopSound, _shake, _weaponSound
            );
            _clientManager.InstallClientListener(_clientListener);

            _gameListener = new ZEGamelistener(_sharedSystem,
                _sharedSystem.GetLoggerFactory().CreateLogger<ZEGamelistener>());
            _modSharp.InstallGameListener(_gameListener);

            return true;
        }

        public void OnAllModulesLoaded()
        {

            _prefs?.WhenAllModulesLoaded();

            _stopSound!.Init();
            _shake!.Init();
            _weaponSound!.Init();
            
        }

        public void PostInit()
        {
        }

        public void Shutdown()
        {

            _clientManager.RemoveClientListener(_clientListener!);
            _modSharp.RemoveGameListener(_gameListener!);

            _stopSound!.Shutdown();
            _shake!.Shutdown();
            _weaponSound!.Shutdown();

            _stopSound = null;
            _shake = null;
            _weaponSound = null;
            _gameListener = null;
            _clientListener = null;

            _prefs!.Shutdown();
            _prefs = null;
                
        }
    }
}
