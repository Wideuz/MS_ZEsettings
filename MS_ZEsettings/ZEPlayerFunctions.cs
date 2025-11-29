using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private ZEClientListener? _clientListener;
        private ZEGamelistener? _gameListener;
        private Prefs? _prefs;

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
            _clientListener = new ZEClientListener(_transmitManager,
                _sharedSystem.GetLoggerFactory().CreateLogger<ZEClientListener>(),
                _sharedSystem,_modSharp);
            _clientManager.InstallClientListener(_clientListener);

            _gameListener = new ZEGamelistener(_sharedSystem,
                _sharedSystem.GetLoggerFactory().CreateLogger<ZEGamelistener>());
            _modSharp.InstallGameListener(_gameListener);

            _prefs = new Prefs(_sharedSystem, "dllPath", "sharpPath", new Version(1, 0),
                coreConfiguration: null!, hotReload: false);
            _prefs.Init();

            return true;
        }

        public void OnAllModulesLoaded()
        {

            _prefs?.WhenAllModulesLoaded();
        }

        public void PostInit()
        {


        }

        public void Shutdown()
        {
            if (_clientListener != null)
            {
                _clientManager.RemoveClientListener(_clientListener);
                _clientListener = null;
            }

            if (_gameListener != null)
            {
                _modSharp.RemoveGameListener(_gameListener);
                _gameListener = null;
            }

            if (_prefs != null)
            {
                _prefs.Shutdown();
                _prefs = null;
            }

        }

        
    }
}
