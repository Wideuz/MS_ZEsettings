using Microsoft.Extensions.Configuration;
using MS_StopMiscSound;
using MS_ZEsettings.Commands;
using Sharp.Modules.ClientPreferences.Shared;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;
using System;

namespace MS_ZEsettings.Preferences
{
    public sealed class Prefs
    {
        // 偏好快取
        public readonly Dictionary<SteamID, bool> _stopSoundPrefs = new();
        public readonly Dictionary<SteamID, bool> _noShakePrefs = new();
        public readonly Dictionary<SteamID, bool> _weaponSoundPrefs = new();
        public readonly Dictionary<SteamID, bool> _footstepPrefs = new();

        private IModSharpModuleInterface<IClientPreference>? _clientPrefsInterface;
        private readonly ISharpModuleManager _moduleManager;
        private IDisposable? _callback;
        private readonly IModSharp _modSharp;

        // 🔔 新增事件，通知外部套用偏好
        public event Action<IGameClient>? PreferencesApplied;

        public Prefs(ISharedSystem sharedSystem, IModSharp modSharp)
        {
            _moduleManager = sharedSystem.GetSharpModuleManager();
            _modSharp = modSharp;
        }

        public bool Init()
        {
            // 如果有需要初始化的邏輯可以放這裡
            return true;
        }

        public void Shutdown()
        {
            _stopSoundPrefs.Clear();
            _noShakePrefs.Clear();
            _weaponSoundPrefs.Clear();
            _footstepPrefs.Clear();
            _callback?.Dispose();
        }


        public void WhenAllModulesLoaded()
        {
            _clientPrefsInterface = _moduleManager.GetRequiredSharpModuleInterface<IClientPreference>(IClientPreference.Identity);
            if (_clientPrefsInterface?.Instance is { } instance)
            {
                _callback = instance.ListenOnLoad(OnCookieLoad);
            }
        }

        private void OnCookieLoad(IGameClient client)
        {
            if (_clientPrefsInterface?.Instance is not { } cp || !cp.IsLoaded(client.SteamId))
                return;

            _stopSoundPrefs[client.SteamId] = (cp.GetCookie(client.SteamId, "StopSound")?.GetNumber() ?? 0) == 1;
            _noShakePrefs[client.SteamId] = (cp.GetCookie(client.SteamId, "NoShake")?.GetNumber() ?? 0) == 1;
            _weaponSoundPrefs[client.SteamId] = (cp.GetCookie(client.SteamId, "WeaponSounds")?.GetNumber() ?? 0) == 1;
            _footstepPrefs[client.SteamId] = (cp.GetCookie(client.SteamId, "FootSteps")?.GetNumber() ?? 0) == 1;

            RecipientFilter filter = new RecipientFilter(client);

            
            bool stopSoundEnabled = GetCachedPreference(client.SteamId, "StopSound");
            _modSharp.PrintChannelFilter(HudPrintChannel.Chat,
                $" {ChatColor.Red}[Prefs] {ChatColor.White} StopSound = {stopSoundEnabled}", filter);
            
            PreferencesApplied?.Invoke(client);
        }

        public void SetPreference(IGameClient client, string key, bool enabled)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                cp.SetCookie(client.SteamId, key, enabled);
            }
        }

        public void SetPreference(IGameClient client, string key, int value)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                cp.SetCookie(client.SteamId, key, value);
            }
        }

        public bool GetPreference(IGameClient client, string key, bool defaultValue = false)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                var cookie = cp.GetCookie(client.SteamId, key);
                if (cookie == null) return defaultValue;

                // 用數字判斷
                return cookie.GetNumber() == 1;
            }
            return defaultValue;
        }


        public int GetPreference(IGameClient client, string key, int defaultValue = 0)
        {
            if (_clientPrefsInterface?.Instance is { } cp && cp.IsLoaded(client.SteamId))
            {
                var cookie = cp.GetCookie(client.SteamId, key);
                if (cookie == null) return defaultValue;

                return (int)cookie.GetNumber();
            }
            return defaultValue;
        }
        public bool GetCachedPreference(SteamID id, string key, bool defaultValue = false)
        {
            return key switch
            {
                "StopSound" => _stopSoundPrefs.TryGetValue(id, out var v1) ? v1 : defaultValue,
                "NoShake" => _noShakePrefs.TryGetValue(id, out var v2) ? v2 : defaultValue,
                "WeaponSounds" => _weaponSoundPrefs.TryGetValue(id, out var v3) ? v3 : defaultValue,
                "FootSteps" => _footstepPrefs.TryGetValue(id, out var v4) ? v4 : defaultValue,
                _ => defaultValue
            };
        }
        public void UpdateCachedPreference(SteamID id, string key, bool value)
        {
            switch (key)
            {
                case "StopSound": _stopSoundPrefs[id] = value; break;
                case "NoShake": _noShakePrefs[id] = value; break;
                case "WeaponSounds": _weaponSoundPrefs[id] = value; break;
                case "FootSteps": _footstepPrefs[id] = value; break;
            }
        }

        public void InitializeClient(SteamID id)
        {
            _stopSoundPrefs[id] = false;
            _noShakePrefs[id] = false;
            _weaponSoundPrefs[id] = false;
            _footstepPrefs[id] = false;
        }
        public void CleanupClient(SteamID id)
        {
            _stopSoundPrefs.Remove(id);
            _noShakePrefs.Remove(id);
            _weaponSoundPrefs.Remove(id);
            _footstepPrefs.Remove(id);
        }
    }
}
