using System;
using Microsoft.Extensions.Configuration;
using Sharp.Modules.ClientPreferences.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace MS_ZEsettings.Preferences
{
    public sealed class Prefs
    {
        private readonly ISharedSystem _sharedSystem;
        private readonly IClientManager _clientManager;
        private readonly IHookManager _hookManager;
        private readonly ISharpModuleManager _moduleManager;

        private IDisposable? _callback;

        // 🔧 ClientPreferences 介面快取
        private IModSharpModuleInterface<IClientPreference>? _clientPrefsInterface;

        public Prefs(ISharedSystem sharedSystem,
            string dllPath,
            string sharpPath,
            Version version,
            IConfiguration coreConfiguration,
            bool hotReload)
        {
            _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
            _clientManager = _sharedSystem.GetClientManager();
            _hookManager = _sharedSystem.GetHookManager();
            _moduleManager = _sharedSystem.GetSharpModuleManager();
        }

        public bool Init()
        {
            
            return true;
        }

        public void Shutdown()
        {
            _callback?.Dispose();
        }

        public void WhenAllModulesLoaded()
        {
            
            _clientPrefsInterface = _moduleManager.GetOptionalSharpModuleInterface<IClientPreference>(IClientPreference.Identity);

            if (_clientPrefsInterface?.Instance is { } instance)
            {
                _callback = instance.ListenOnLoad(OnCookieLoad);
            }
        }

        public void OnLibraryConnected(string name)
        {
            if (name.Equals("ClientPreferences"))
            {
                _clientPrefsInterface = _moduleManager.GetRequiredSharpModuleInterface<IClientPreference>(IClientPreference.Identity);

                if (_clientPrefsInterface?.Instance is { } instance)
                {
                    _callback = instance.ListenOnLoad(OnCookieLoad);
                }
            }
        }

        public void OnLibraryDisconnect(string name)
        {
            if (name.Equals("ClientPreferences"))
            {
                _clientPrefsInterface = null;
            }
        }

        private void OnCookieLoad(IGameClient client)
        {
            if (_clientPrefsInterface?.Instance is not { } cp) return;

            // 讀取玩家偏好
            if (cp.GetCookie(client.SteamId, "MySetting") is { } cookie)
            {
                Console.WriteLine($"[Prefs] Loaded setting: {cookie.GetString()}");
            }
            else
            {
                // 沒有設定就寫入預設值
                cp.SetCookie(client.SteamId, "MySetting", "default");
            }
        }

        
        public void SetPreference(IGameClient client, string key, string value)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                cp.SetCookie(client.SteamId, key, value);
            }
        }

        
        public string? GetPreference(IGameClient client, string key)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                return cp.GetCookie(client.SteamId, key)?.GetString();
            }
            return null;
        }
    }
}