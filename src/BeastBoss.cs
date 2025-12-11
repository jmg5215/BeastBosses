using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using UnityEngine;
using Rust; // DamageType, MapMarkerGenericRadius, etc.
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("BeastBoss", "Somescrub", "0.4.0")]
    [Description("BossMonster-style animal bosses with tiers, lockouts, spawnpoints, events and a boss health HUD")]
    public class BeastBoss : RustPlugin
    {
        #region Config

        private ConfigData _config;

        public class ConfigData
        {
            public Dictionary<string, BeastDef> Beasts = new Dictionary<string, BeastDef>
            {
                ["bear"] = new BeastDef
                {
                    DisplayName = "Dire Bear",
                    TierId = "T1",
                    Theme = "fire",
                    Prefab = "assets/rust.ai/agents/bear/bear.prefab",
                    BaseHealth = 2500f,
                    DamageMultiplier = 1.5f,
                    InitialScale = 1.2f,
                    EnragedScale = 1.5f,

                    PhaseScales = new List<PhaseScale>
                    {
                        new PhaseScale { HealthFraction = 0.75f, Scale = 1.3f },
                        new PhaseScale { HealthFraction = 0.50f, Scale = 1.4f },
                        new PhaseScale { HealthFraction = 0.25f, Scale = 1.5f }
                    },

                    AbilityRoar = new RoarSettings
                    {
                        Enabled = true,
                        Interval = 12f,
                        Radius = 10f,
                        Damage = 10f,
                        Bleed = 10f,
                        ScreenShake = 2.0f
                    },

                    AbilityCharge = new ChargeSettings
                    {
                        Enabled = true,
                        Interval = 20f,
                        Range = 25f,
                        ImpactRadius = 3.0f,
                        ImpactDamage = 40f,
                        ChargeForce = 13f
                    },

                    AbilityFrostAura = new FrostAuraSettings
                    {
                        Enabled = false,
                        Interval = 18f,
                        Radius = 9f,
                        Duration = 4f,
                        TickRate = 1f,
                        DamagePerTick = 4f,
                        ColdPerTick = 10f
                    },

                    AbilityCubSummon = new CubSummonSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.5f,
                        Count = 3,
                        Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                        Radius = 6f
                    },

                    AbilityEnrage = new EnrageSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.3f,
                        DamageMultiplier = 1.6f,
                        SpeedMultiplier = 1.4f,
                        EffectPrefab = "assets/bundled/prefabs/fx/gestures/flex.prefab"
                    },

                    AbilityFireTrail = new FireTrailSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Duration = 5f,
                        Step = 0.7f,
                        Radius = 3f,
                        DamagePerStep = 6f
                    },

                    Loot = new List<LootEntry>
                    {
                        new LootEntry { ShortName = "hq.metal.ore", Amount = 200, Skin = 0 },
                        new LootEntry { ShortName = "bearmeat", Amount = 15, Skin = 0 },
                        new LootEntry { ShortName = "scrap", Amount = 150, Skin = 0 }
                    }
                },

                ["wolf"] = new BeastDef
                {
                    DisplayName = "Nightfang",
                    TierId = "T1",
                    Theme = "frost",
                    Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                    BaseHealth = 1500f,
                    DamageMultiplier = 1.3f,
                    InitialScale = 1.1f,
                    EnragedScale = 1.4f,

                    PhaseScales = new List<PhaseScale>
                    {
                        new PhaseScale { HealthFraction = 0.50f, Scale = 1.2f },
                        new PhaseScale { HealthFraction = 0.25f, Scale = 1.3f }
                    },

                    AbilityRoar = new RoarSettings
                    {
                        Enabled = true,
                        Interval = 14f,
                        Radius = 8f,
                        Damage = 7f,
                        Bleed = 7f,
                        ScreenShake = 1.2f
                    },

                    AbilityCharge = new ChargeSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Range = 20f,
                        ImpactRadius = 2.5f,
                        ImpactDamage = 30f,
                        ChargeForce = 15f
                    },

                    AbilityFrostAura = new FrostAuraSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Radius = 10f,
                        Duration = 4f,
                        TickRate = 1f,
                        DamagePerTick = 5f,
                        ColdPerTick = 15f
                    },

                    AbilityCubSummon = new CubSummonSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.4f,
                        Count = 4,
                        Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                        Radius = 8f
                    },

                    AbilityEnrage = new EnrageSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.25f,
                        DamageMultiplier = 1.7f,
                        SpeedMultiplier = 1.5f,
                        EffectPrefab = "assets/bundled/prefabs/fx/gestures/howl.prefab"
                    },

                    AbilityFireTrail = new FireTrailSettings
                    {
                        Enabled = false,
                        Interval = 18f,
                        Duration = 4f,
                        Step = 0.7f,
                        Radius = 3f,
                        DamagePerStep = 5f
                    },

                    Loot = new List<LootEntry>
                    {
                        new LootEntry { ShortName = "leather", Amount = 100, Skin = 0 },
                        new LootEntry { ShortName = "wolfmeat", Amount = 10, Skin = 0 },
                        new LootEntry { ShortName = "scrap", Amount = 80, Skin = 0 }
                    }
                }
            };

            public Dictionary<string, TierConfig> Tiers = new Dictionary<string, TierConfig>
            {
                ["T1"] = new TierConfig
                {
                    DisplayName = "Tier 1 Beasts",
                    LockoutMinutes = 60.0,
                    AutoSpawn = false,
                    AutoRespawnMinutes = 30.0
                }
            };

            public float GlobalIncomingDamageMultiplier = 0.8f;
            public float GlobalOutgoingDamageMultiplier = 1.0f;
            public float AnnounceRadius = 80f;
            public string ChatPrefix = "<color=#d57b00>[Beast]</color> ";
            public string PermissionAdmin = "beastboss.admin";

            public float EventCheckIntervalSeconds = 60f;

            // Enable phase-based scaling (beasts scale as health drops to phases).
            // If false, only InitialScale and EnragedScale are applied.
            public bool UsePhaseScaling = false;

            // Boss HUD config (roughly compatible look with TargetHealthHUD)
            public UiConfig Ui = new UiConfig();

            /*
             * FX Library keys and usage:
             *  - "roar_blast"   : Used by DoRoar() for the main roar visual + optional impact FX.
             *  - "fire_trail"   : Used by DoFireTrail() for the trail FX behind fire-themed beasts.
             *  - "frost_aura"   : Used by DoFrostAura() as the primary cold aura visual.
             *  - "toxic_aura"   : Optional alternative to frost_aura for poison-themed beasts.
             *  - "ground_impact": Used by DoRoar() and other impact-style abilities for ground shock FX.
             *  - "enrage_burst" : Used by TriggerEnrage() for the enrage burst visual.
             *  - "summon_burst" : Used by DoCubSummon() when spawning minions.
             *
             * These keys are defined in ConfigData.FxLibrary and can be customized in BeastBoss.json
             * without changing code.
             */
            public Dictionary<string, List<string>> FxLibrary = new Dictionary<string, List<string>>
            {
                // Generic categories used by abilities
                ["roar_blast"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/gestures/howl.prefab",
                    "assets/bundled/prefabs/fx/dustwave.prefab"
                },
                ["fire_trail"] = new List<string>
                {
                    "assets/bundled/prefabs/fireball_small.prefab",
                    "assets/bundled/prefabs/fireball_medium.prefab"
                },
                ["frost_aura"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/hold_breath.prefab"
                },
                ["toxic_aura"] = new List<string>
                {
                    "assets/content/effects/prefabs/poisoncloud.prefab",
                    "assets/content/effects/prefabs/green_spores.prefab"
                },
                ["ground_impact"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/groundimpact.prefab",
                    "assets/bundled/prefabs/fx/dustwave.prefab"
                },
                ["enrage_burst"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/gestures/flex.prefab",
                    "assets/bundled/prefabs/fireball_small.prefab"
                },
                ["summon_burst"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/takeloot.prefab",
                    "assets/bundled/prefabs/fx/item_pickup.prefab"
                }
            };
        }

        public class UiConfig
        {
            public bool Enabled = true;
            public string Style = "top detailed";
            public string TextFormat = "{0} {1}/{2} HP";
            public int ActiveDuration = 10; // seconds to keep HUD after last hit

            public string TextColor = "1 1 1 1";
            public string PrimaryColor = "0.7058824 0.07843138 0.07843138 1";
            public string SecondaryColor = "1 0.1960784 0.1960784 1";

            // Slightly below your TargetHealthHUD default so they don't overlap perfectly
            public string AnchorMin = "0.3297916 0.88";
            public string AnchorMax = "0.6683334 0.91";
        }

        public class TierConfig
        {
            public string DisplayName;
            public double LockoutMinutes;
            public bool AutoSpawn;
            public double AutoRespawnMinutes;
        }

        public class BeastDef
        {
            public string DisplayName;
            public string TierId;
            public string Theme;
            public string Prefab;
            public float BaseHealth;
            public float DamageMultiplier;
            public float InitialScale = 1f;
            public float EnragedScale = 1f;

            public RoarSettings AbilityRoar = new RoarSettings();
            public ChargeSettings AbilityCharge = new ChargeSettings();
            public FrostAuraSettings AbilityFrostAura = new FrostAuraSettings();
            public CubSummonSettings AbilityCubSummon = new CubSummonSettings();
            public EnrageSettings AbilityEnrage = new EnrageSettings();
            public FireTrailSettings AbilityFireTrail = new FireTrailSettings();

            public List<LootEntry> Loot = new List<LootEntry>();
            public List<PhaseScale> PhaseScales = new List<PhaseScale>();
        }

        public class RoarSettings
        {
            public bool Enabled;
            public float Interval;
            public float Radius;
            public float Damage;
            public float Bleed;
            public float ScreenShake;
        }

        public class ChargeSettings
        {
            public bool Enabled;
            public float Interval;
            public float Range;
            public float ImpactRadius;
            public float ImpactDamage;
            public float ChargeForce;
        }

        public class FrostAuraSettings
        {
            public bool Enabled;
            public float Interval;
            public float Radius;
            public float Duration;
            public float TickRate;
            public float DamagePerTick;
            public float ColdPerTick;
        }

        public class CubSummonSettings
        {
            public bool Enabled;
            public float TriggerHealthFraction;
            public int Count;
            public string Prefab;
            public float Radius;
        }

        public class EnrageSettings
        {
            public bool Enabled;
            public float TriggerHealthFraction;
            public float DamageMultiplier;
            public float SpeedMultiplier;
            public string EffectPrefab;
        }

        public class FireTrailSettings
        {
            public bool Enabled;
            public float Interval;
            public float Duration;
            public float Step;
            public float Radius;
            public float DamagePerStep;
        }

        public class PhaseScale
        {
            // Health fraction at or below which this phase is triggered (0-1, e.g. 0.75 = 75% HP).
            public float HealthFraction;

            // Scale to apply when this phase triggers (e.g. 1.1, 1.3, 1.5).
            public float Scale;
        }

        public class LootEntry
        {
            public string ShortName;
            public int Amount;
            public ulong Skin;
        }

        protected override void LoadDefaultConfig()
        {
            _config = new ConfigData();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<ConfigData>();
                if (_config == null) throw new Exception();
            }
            catch
            {
                PrintWarning("Invalid config, generating new one.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(_config, true);

        #endregion

        #region Data & State

        private class Vector3Serializable
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToVector3() => new Vector3(x, y, z);

            public static Vector3Serializable FromVector3(Vector3 v) =>
                new Vector3Serializable { x = v.x, y = v.y, z = v.z };
        }

        private class StoredData
        {
            public Dictionary<string, List<Vector3Serializable>> TierSpawns =
                new Dictionary<string, List<Vector3Serializable>>();

            public Dictionary<string, Dictionary<ulong, double>> TierLockouts =
                new Dictionary<string, Dictionary<ulong, double>>();
        }

        private StoredData _data;

        private const string ExternalHudPermission = "targethealthhud.use";

        // Players for whom we've temporarily revoked TargetHealthHUD permission
        private readonly HashSet<ulong> _suspendedExternalHud = new HashSet<ulong>();

        private readonly HashSet<BaseEntity> _bosses = new HashSet<BaseEntity>();
        private readonly Dictionary<ulong, float> _damageMeter = new Dictionary<ulong, float>();
        private readonly Dictionary<NetworkableId, BeastDef> _beastDefs = new Dictionary<NetworkableId, BeastDef>();
        private readonly Dictionary<NetworkableId, float> _bossDamageMultipliers = new Dictionary<NetworkableId, float>();

        private readonly Dictionary<string, BaseEntity> _activeBossByTier = new Dictionary<string, BaseEntity>();
        private readonly Dictionary<NetworkableId, string> _bossTierById = new Dictionary<NetworkableId, string>();
        private readonly Dictionary<string, double> _tierLastDeathTime = new Dictionary<string, double>();
        private readonly Dictionary<NetworkableId, MapMarkerGenericRadius> _bossMarkers =
            new Dictionary<NetworkableId, MapMarkerGenericRadius>();

        // HUD tracking: player -> boss & timers
        private readonly Dictionary<ulong, BaseCombatEntity> _hudBossTargets = new Dictionary<ulong, BaseCombatEntity>();
        private readonly Dictionary<ulong, Timer> _hudUpdateTimers = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> _hudExpireTimers = new Dictionary<ulong, Timer>();

        private void LoadData()
        {
            try
            {
                _data = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("BeastBossData");
            }
            catch
            {
                _data = new StoredData();
            }

            if (_data.TierSpawns == null)
                _data.TierSpawns = new Dictionary<string, List<Vector3Serializable>>();
            if (_data.TierLockouts == null)
                _data.TierLockouts = new Dictionary<string, Dictionary<ulong, double>>();
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("BeastBossData", _data);
        }

        #endregion

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(_config.PermissionAdmin, this);
            LoadData();
            StartAutoSpawnTimer();
        }

        private void Unload()
        {
            foreach (var boss in _bosses.ToList())
            {
                if (boss == null) continue;
                var comp = boss.GetComponent<BeastComponent>();
                if (comp != null) UnityEngine.Object.Destroy(comp);
            }

            foreach (var marker in _bossMarkers.Values)
            {
                if (marker != null && !marker.IsDestroyed)
                    marker.Kill();
            }

            foreach (var player in BasePlayer.activePlayerList)
            {
                RemoveHud(player);
            }

            _bosses.Clear();
            _beastDefs.Clear();
            _bossDamageMultipliers.Clear();
            _activeBossByTier.Clear();
            _bossTierById.Clear();
            _bossMarkers.Clear();

            foreach (var userId in _suspendedExternalHud.ToList())
            {
                RestoreExternalHud(userId);
            }
            _suspendedExternalHud.Clear();

            SaveData();
        }

        private void StartAutoSpawnTimer()
        {
            if (_config.EventCheckIntervalSeconds <= 0f)
                _config.EventCheckIntervalSeconds = 60f;

            timer.Every(_config.EventCheckIntervalSeconds, CheckAutoSpawns);
        }

        private void CheckAutoSpawns()
        {
            var now = Interface.Oxide.Now;

            foreach (var kv in _config.Tiers)
            {
                var tierId = kv.Key;
                var tier = kv.Value;

                if (!tier.AutoSpawn)
                    continue;

                if (_activeBossByTier.TryGetValue(tierId, out var existing) &&
                    existing != null && !existing.IsDestroyed)
                    continue;

                _tierLastDeathTime.TryGetValue(tierId, out var lastDeath);
                var respawnDelaySeconds = Math.Max(0, tier.AutoRespawnMinutes) * 60;
                if (now - lastDeath < respawnDelaySeconds)
                    continue;

                if (!_data.TierSpawns.TryGetValue(tierId, out var spawns) || spawns.Count == 0)
                    continue;

                var beastsInTier = _config.Beasts.Values
                    .Where(b => string.Equals(b.TierId, tierId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (beastsInTier.Count == 0)
                    continue;

                var spawn = spawns.GetRandom();
                var pos = spawn.ToVector3();
                pos.y = TerrainMeta.HeightMap.GetHeight(pos);

                var def = beastsInTier.GetRandom();
                SpawnBeast(def, pos);
            }
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;

            // Outgoing damage from boss
            if (info.Initiator != null && _bosses.Contains(info.Initiator))
            {
                var initiatorId = info.Initiator.net.ID;
                float mult = 1f;
                if (_bossDamageMultipliers.TryGetValue(initiatorId, out var m))
                    mult = m;

                info.damageTypes.ScaleAll(_config.GlobalOutgoingDamageMultiplier * mult);
            }

            // Incoming damage to boss
            if (_bosses.Contains(entity))
            {
                BeastDef def;
                _beastDefs.TryGetValue(entity.net.ID, out def);
                var tierId = def != null ? def.TierId : null;

                var initiatorPlayer = info.InitiatorPlayer;
                if (initiatorPlayer != null && !string.IsNullOrEmpty(tierId))
                {
                    if (IsPlayerLockedOut(initiatorPlayer.userID, tierId, out var remainingSeconds))
                    {
                        info.damageTypes.ScaleAll(0f);
                        var mins = Mathf.CeilToInt((float)(remainingSeconds / 60d));
                        initiatorPlayer.ChatMessage(
                            $"{_config.ChatPrefix}You are locked out of {tierId} for {mins} more minute(s).");
                        return;
                    }
                }

                info.damageTypes.ScaleAll(_config.GlobalIncomingDamageMultiplier);

                if (initiatorPlayer != null)
                {
                    var dmg = info.damageTypes.Total();
                    if (!_damageMeter.ContainsKey(initiatorPlayer.userID))
                        _damageMeter[initiatorPlayer.userID] = 0f;

                    _damageMeter[initiatorPlayer.userID] += dmg;

                    // Boss HUD tracking
                    if (_config.Ui.Enabled)
                        TrackBossHud(initiatorPlayer, entity);
                }
            }
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null) return;
            if (!_bosses.Contains(entity)) return;

            _bosses.Remove(entity);

            BeastDef def;
            if (!_beastDefs.TryGetValue(entity.net.ID, out def)) return;

            string tierId = def.TierId;
            var now = Interface.Oxide.Now;

            if (!string.IsNullOrEmpty(tierId))
            {
                if (_activeBossByTier.TryGetValue(tierId, out var existing) && existing == entity)
                    _activeBossByTier.Remove(tierId);

                _tierLastDeathTime[tierId] = now;
            }

            AnnounceNearby(entity.transform.position, $"{def.DisplayName} has been slain!");
            DropConfiguredLoot(entity.transform.position, def);

            if (!string.IsNullOrEmpty(tierId))
            {
                foreach (var kv in _damageMeter)
                {
                    AddLockout(kv.Key, tierId);
                }
            }

            if (_damageMeter.Count > 0)
            {
                var top = _damageMeter.OrderByDescending(kv => kv.Value).First();
                var topPlayer = BasePlayer.FindByID(top.Key);
                if (topPlayer != null)
                {
                    PrintToChat($"{_config.ChatPrefix}{topPlayer.displayName} dealt the most damage ({Mathf.RoundToInt(top.Value)})!");
                }
            }

            _damageMeter.Clear();
            _beastDefs.Remove(entity.net.ID);
            _bossDamageMultipliers.Remove(entity.net.ID);

            if (_bossMarkers.TryGetValue(entity.net.ID, out var marker) && marker != null && !marker.IsDestroyed)
            {
                marker.Kill();
            }
            _bossMarkers.Remove(entity.net.ID);
            _bossTierById.Remove(entity.net.ID);

            // Clear HUD for any players tracking this boss
            ClearHudForBoss(entity);

            SaveData();
        }

        #endregion

        #region Commands – Admin

        [ChatCommand("beastspawn")]
        private void CmdBeastSpawn(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, $"{_config.ChatPrefix}Usage: /beastspawn <beastKey>");
                SendReply(player, $"{_config.ChatPrefix}Available keys: {string.Join(", ", _config.Beasts.Keys)}");
                return;
            }

            var key = args[0].ToLowerInvariant();
            if (!_config.Beasts.TryGetValue(key, out var def))
            {
                SendReply(player, $"{_config.ChatPrefix}Unknown beast '{key}'.");
                return;
            }

            var pos = GetSpawnPosition(player);
            var entity = SpawnBeast(def, pos);
            if (entity == null)
            {
                SendReply(player, $"{_config.ChatPrefix}Failed to spawn beast.");
            }
        }

        [ChatCommand("beastaddspawn")]
        private void CmdBeastAddSpawn(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, $"{_config.ChatPrefix}Usage: /beastaddspawn <tierId>");
                return;
            }

            var tierId = args[0];
            if (!_config.Tiers.ContainsKey(tierId))
            {
                SendReply(player, $"{_config.ChatPrefix}Tier '{tierId}' is not defined in config.");
                return;
            }

            if (!_data.TierSpawns.TryGetValue(tierId, out var list))
            {
                list = new List<Vector3Serializable>();
                _data.TierSpawns[tierId] = list;
            }

            var pos = player.transform.position;
            list.Add(Vector3Serializable.FromVector3(pos));
            SaveData();

            SendReply(player, $"{_config.ChatPrefix}Added spawnpoint for tier '{tierId}' at {pos}.");
        }

        [ChatCommand("beastclearspawns")]
        private void CmdBeastClearSpawns(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, $"{_config.ChatPrefix}Usage: /beastclearspawns <tierId>");
                return;
            }

            var tierId = args[0];
            if (_data.TierSpawns.Remove(tierId))
            {
                SaveData();
                SendReply(player, $"{_config.ChatPrefix}Cleared all spawnpoints for tier '{tierId}'.");
            }
            else
            {
                SendReply(player, $"{_config.ChatPrefix}No spawnpoints found for tier '{tierId}'.");
            }
        }

        [ChatCommand("beastspawns")]
        private void CmdBeastSpawns(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (_data.TierSpawns.Count == 0)
            {
                SendReply(player, $"{_config.ChatPrefix}No spawnpoints defined.");
                return;
            }

            SendReply(player, $"{_config.ChatPrefix}Spawnpoints by tier:");
            foreach (var kv in _data.TierSpawns)
            {
                SendReply(player, $"- {kv.Key}: {kv.Value.Count} spawn(s)");
            }
        }

        #endregion

        #region Commands – Player

        [ChatCommand("beastlock")]
        private void CmdBeastLock(BasePlayer player, string command, string[] args)
        {
            var userId = player.userID;
            var now = Interface.Oxide.Now;
            var lines = new List<string>();

            foreach (var kv in _data.TierLockouts)
            {
                var tierId = kv.Key;
                var dict = kv.Value;

                if (!dict.TryGetValue(userId, out var expiry))
                    continue;

                var remaining = expiry - now;
                if (remaining <= 0) continue;

                var mins = Mathf.CeilToInt((float)(remaining / 60d));
                string tierName = tierId;
                if (_config.Tiers.TryGetValue(tierId, out var tierCfg) && !string.IsNullOrEmpty(tierCfg.DisplayName))
                    tierName = tierCfg.DisplayName;

                lines.Add($"{tierName}: {mins} minute(s) remaining");
            }

            if (lines.Count == 0)
            {
                SendReply(player, $"{_config.ChatPrefix}You have no active beast lockouts.");
            }
            else
            {
                SendReply(player, $"{_config.ChatPrefix}Your active beast lockouts:");
                foreach (var line in lines)
                    SendReply(player, "- " + line);
            }
        }

        #endregion

        #region Helpers

        private Vector3 GetSpawnPosition(BasePlayer player)
        {
            var pos = player.transform.position + player.eyes.BodyForward() * 6f;
            pos.y = TerrainMeta.HeightMap.GetHeight(pos);
            return pos;
        }

        private void AnnounceNearby(Vector3 pos, string message)
        {
            foreach (var p in BasePlayer.activePlayerList)
            {
                if (p == null || !p.IsConnected) continue;
                if (Vector3.Distance(pos, p.transform.position) <= _config.AnnounceRadius)
                {
                    PrintToChat(p, _config.ChatPrefix + message);
                }
            }
        }

        private void SuspendExternalHud(BasePlayer player)
        {
            if (player == null) return;

            var userId = player.userID;
            var userIdStr = player.UserIDString;

            // If we've already suspended for this player, do nothing
            if (_suspendedExternalHud.Contains(userId))
                return;

            // Only revoke if they currently have the permission
            if (!permission.UserHasPermission(userIdStr, ExternalHudPermission))
                return;

            _suspendedExternalHud.Add(userId);
            permission.RevokeUserPermission(userIdStr, ExternalHudPermission);
        }

        private void RestoreExternalHud(ulong userId)
        {
            if (!_suspendedExternalHud.Contains(userId))
                return;

            _suspendedExternalHud.Remove(userId);

            var userIdStr = userId.ToString();

            // Only grant back if they don't already have it
            if (!permission.UserHasPermission(userIdStr, ExternalHudPermission))
            {
                permission.GrantUserPermission(userIdStr, ExternalHudPermission, this);
            }
        }

        private BaseEntity SpawnBeast(BeastDef def, Vector3 pos)
        {
            var entity = GameManager.server.CreateEntity(def.Prefab, pos, Quaternion.identity, true);
            if (entity == null) return null;

            entity.enableSaving = false;
            entity.Spawn();

            var combat = entity as BaseCombatEntity;
            if (combat != null)
            {
                combat.InitializeHealth(def.BaseHealth, def.BaseHealth);
            }

            // Apply initial scale
            ScaleBeastEntity(entity, def.InitialScale);

            var driver = entity.gameObject.AddComponent<BeastComponent>();
            driver.Init(this, entity, def);

            _bosses.Add(entity);
            _beastDefs[entity.net.ID] = def;
            _bossDamageMultipliers[entity.net.ID] = def.DamageMultiplier;

            if (!string.IsNullOrEmpty(def.TierId))
            {
                _activeBossByTier[def.TierId] = entity;
                _bossTierById[entity.net.ID] = def.TierId;
                _tierLastDeathTime[def.TierId] = Interface.Oxide.Now;
            }

            var marker = GameManager.server.CreateEntity(
                "assets/prefabs/tools/map/genericradiusmarker.prefab",
                pos,
                Quaternion.identity,
                true
            ) as MapMarkerGenericRadius;

            if (marker != null)
            {
                marker.alpha = 1f;
                marker.radius = 0.5f;
                marker.color1 = Color.red;
                marker.Spawn();
                _bossMarkers[entity.net.ID] = marker;
            }

            AnnounceNearby(entity.transform.position, $"<color=#ffdd66>{def.DisplayName}</color> has appeared!");

            return entity;
        }

        private void DropConfiguredLoot(Vector3 pos, BeastDef def)
        {
            var container = new ItemContainer();
            int slots = Mathf.Max(6, def.Loot.Count);
            container.ServerInitialize(null, slots);
            container.GiveUID();

            foreach (var entry in def.Loot)
            {
                var defItem = ItemManager.FindItemDefinition(entry.ShortName);
                if (defItem == null) continue;
                var item = ItemManager.Create(defItem, entry.Amount, entry.Skin);
                item?.MoveToContainer(container);
            }

            var dropEntity = GameManager.server.CreateEntity(
                "assets/prefabs/misc/item drop/item_drop_backpack.prefab",
                pos,
                Quaternion.identity,
                true
            ) as DroppedItemContainer;

            if (dropEntity == null)
            {
                container.Kill();
                return;
            }

            dropEntity.inventory = container;
            container.entityOwner = dropEntity;
            dropEntity.ResetRemovalTime();
            dropEntity.Spawn();
        }

        private bool IsPlayerLockedOut(ulong userId, string tierId, out double remainingSeconds)
        {
            remainingSeconds = 0;
            if (string.IsNullOrEmpty(tierId)) return false;

            if (_data.TierLockouts.TryGetValue(tierId, out var dict) &&
                dict.TryGetValue(userId, out var expiry))
            {
                var now = Interface.Oxide.Now;
                remainingSeconds = expiry - now;
                if (remainingSeconds > 0) return true;

                dict.Remove(userId);
                SaveData();
            }

            return false;
        }

        private void AddLockout(ulong userId, string tierId)
        {
            if (string.IsNullOrEmpty(tierId)) return;
            if (!_config.Tiers.TryGetValue(tierId, out var tierCfg)) return;

            var minutes = Math.Max(0, tierCfg.LockoutMinutes);
            if (minutes <= 0) return;

            if (!_data.TierLockouts.TryGetValue(tierId, out var dict))
            {
                dict = new Dictionary<ulong, double>();
                _data.TierLockouts[tierId] = dict;
            }

            var now = Interface.Oxide.Now;
            dict[userId] = now + minutes * 60.0;
        }

        internal void ApplyEnrageBuff(BaseEntity entity, BeastDef def)
        {
            if (entity == null) return;
            if (!_bossDamageMultipliers.ContainsKey(entity.net.ID))
                _bossDamageMultipliers[entity.net.ID] = def.DamageMultiplier;

            _bossDamageMultipliers[entity.net.ID] *= def.AbilityEnrage.DamageMultiplier;
        }

        internal void PrintBossChat(string message)
        {
            PrintToChat(_config.ChatPrefix + message);
        }

        private string GetRandomFx(string key, string fallback = null, string theme = null)
        {
            if (_config != null && _config.FxLibrary != null)
            {
                // If theme is provided, try themed key first (e.g., "fire_roar_blast")
                if (!string.IsNullOrEmpty(theme))
                {
                    var themedKey = theme + "_" + key;
                    if (_config.FxLibrary.TryGetValue(themedKey, out var themedList) &&
                        themedList != null &&
                        themedList.Count > 0)
                    {
                        return themedList.GetRandom();
                    }
                }

                // Fall back to generic key (e.g., "roar_blast")
                if (_config.FxLibrary.TryGetValue(key, out var list) &&
                    list != null &&
                    list.Count > 0)
                {
                    return list.GetRandom();
                }
            }

            return fallback;
        }

        private void ScaleBeastEntity(BaseEntity entity, float scale)
        {
            if (entity == null)
                return;

            // Reject non-positive / NaN scales.
            if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
                return;

            // Keep bosses "modestly huge":
            //  - MinScale slightly below 1 so minor shrink is possible if desired.
            //  - MaxScale around 1.75 to avoid navmesh/physics issues and visual jank.
            const float MinScale = 0.9f;
            const float MaxScale = 1.75f;

            var clampedScale = Mathf.Clamp(scale, MinScale, MaxScale);

            // If the clamped scale is effectively 1, don't bother changing it.
            if (Mathf.Approximately(clampedScale, 1f))
                return;

            // Try to use EntityScaleManager API if that plugin is installed.
            var result = Interface.CallHook("API_ScaleEntity", entity, clampedScale);
            if (result is bool b && b)
                return;

            // Fallback: use native scaling directly.
            var targetScale = Vector3.one * clampedScale;

            if (entity.transform.localScale == targetScale && entity.networkEntityScale)
                return;

            entity.transform.localScale = targetScale;
            entity.networkEntityScale = true;
            entity.SendNetworkUpdate();
        }

        #endregion

        #region HUD Logic

        private void TrackBossHud(BasePlayer player, BaseCombatEntity boss)
        {
            if (player == null || boss == null || boss.IsDestroyed)
                return;

            var userId = player.userID;
            _hudBossTargets[userId] = boss;

            SuspendExternalHud(player);

            // Ensure update timer
            if (!_hudUpdateTimers.ContainsKey(userId))
            {
                _hudUpdateTimers[userId] = timer.Every(0.2f, () =>
                {
                    var p = BasePlayer.FindByID(userId);
                    if (p == null || !p.IsConnected)
                    {
                        RemoveHud(userId);
                        return;
                    }

                    if (!_hudBossTargets.TryGetValue(userId, out var target) ||
                        target == null || target.IsDestroyed)
                    {
                        RemoveHud(p);
                        return;
                    }

                    ShowBossHud(p, target);
                });
            }

            // Reset expire timer
            if (_hudExpireTimers.ContainsKey(userId))
            {
                _hudExpireTimers[userId]?.Destroy();
                _hudExpireTimers.Remove(userId);
            }

            var duration = Math.Max(1, _config.Ui.ActiveDuration);
            _hudExpireTimers[userId] = timer.Once(duration, () =>
            {
                var p = BasePlayer.FindByID(userId);
                if (p != null && p.IsConnected)
                    RemoveHud(p);
                else
                    RemoveHud(userId);
            });
        }

        private void ShowBossHud(BasePlayer player, BaseCombatEntity boss)
        {
            if (!_config.Ui.Enabled) return;

            float current = boss.health;
            float max = boss.MaxHealth();
            if (max <= 0f) max = 1f;

            float percent = Mathf.Clamp01(current / max);

            string name = boss.ShortPrefabName;
            if (_beastDefs.TryGetValue(boss.net.ID, out var def))
                name = def.DisplayName;

            string text = string.Format(_config.Ui.TextFormat, name, Mathf.CeilToInt(current), Mathf.CeilToInt(max));

            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = false,
                Image = { Color = _config.Ui.SecondaryColor },
                RectTransform = { AnchorMin = _config.Ui.AnchorMin, AnchorMax = _config.Ui.AnchorMax, OffsetMin = "", OffsetMax = "" }
            }, "Hud", "BeastBossHUD");

            container.Add(new CuiPanel
            {
                CursorEnabled = false,
                Image = { Color = _config.Ui.PrimaryColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = $"{percent} 1", OffsetMin = "", OffsetMax = "" }
            }, "BeastBossHUD", "BeastBossHUD_Cover");

            container.Add(new CuiElement
            {
                Name = "BeastBossHUD_Label",
                Parent = "BeastBossHUD",
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = text,
                        Font = "robotocondensed-bold.ttf",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter,
                        Color = _config.Ui.TextColor
                    },
                    new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1", OffsetMin = "", OffsetMax = "" }
                }
            });

            CuiHelper.DestroyUi(player, "BeastBossHUD");
            CuiHelper.AddUi(player, container);
        }

        private void ClearHudForBoss(BaseEntity boss)
        {
            if (boss == null) return;

            foreach (var kv in _hudBossTargets.ToList())
            {
                if (kv.Value == boss)
                {
                    var player = BasePlayer.FindByID(kv.Key);
                    if (player != null && player.IsConnected)
                        RemoveHud(player);
                    else
                        RemoveHud(kv.Key);
                }
            }
        }

        private void RemoveHud(BasePlayer player)
        {
            if (player == null) return;
            var id = player.userID;
            CuiHelper.DestroyUi(player, "BeastBossHUD");
            RemoveHud(id);
            RestoreExternalHud(id);
        }

        private void RemoveHud(ulong userId)
        {
            if (_hudUpdateTimers.TryGetValue(userId, out var t))
            {
                t.Destroy();
                _hudUpdateTimers.Remove(userId);
            }

            if (_hudExpireTimers.TryGetValue(userId, out var e))
            {
                e.Destroy();
                _hudExpireTimers.Remove(userId);
            }

            if (_hudBossTargets.ContainsKey(userId))
                _hudBossTargets.Remove(userId);
        }

        #endregion

        #region BeastComponent

        private class BeastComponent : MonoBehaviour
        {
            private BeastBoss _plugin;
            private BaseEntity _entity;
            private BaseCombatEntity _combat;
            private BaseAnimalNPC _animal;
            private BeastDef _def;

            private float _nextRoar;
            private float _nextCharge;
            private float _nextFrost;
            private float _nextFireTrail;

            private bool _summonedCubs;
            private bool _enraged;
            private readonly HashSet<float> _triggeredPhaseScales = new HashSet<float>();

            public void Init(BeastBoss plugin, BaseEntity entity, BeastDef def)
            {
                _plugin = plugin;
                _entity = entity;
                _combat = entity as BaseCombatEntity;
                _animal = entity as BaseAnimalNPC;
                _def = def;

                var now = Time.realtimeSinceStartup;
                _nextRoar = now + UnityEngine.Random.Range(4f, 8f);
                _nextCharge = now + UnityEngine.Random.Range(8f, 12f);
                _nextFrost = now + UnityEngine.Random.Range(10f, 14f);
                _nextFireTrail = now + UnityEngine.Random.Range(10f, 16f);

                InvokeRepeating(nameof(Tick), 0.5f, 0.25f);
            }

            private void OnDestroy()
            {
                CancelInvoke(nameof(Tick));
            }

            private void Tick()
            {
                if (_entity == null || _entity.IsDestroyed)
                {
                    CancelInvoke(nameof(Tick));
                    return;
                }

                var now = Time.realtimeSinceStartup;

                if (_combat != null)
                {
                    var frac = _combat.health / Mathf.Max(1f, _def.BaseHealth);

                    if (_def.AbilityCubSummon.Enabled && !_summonedCubs &&
                        frac <= Mathf.Clamp01(_def.AbilityCubSummon.TriggerHealthFraction))
                    {
                        DoCubSummon();
                        _summonedCubs = true;
                    }

                    if (_def.AbilityEnrage.Enabled && !_enraged &&
                        frac <= Mathf.Clamp01(_def.AbilityEnrage.TriggerHealthFraction))
                    {
                        TriggerEnrage();
                        _enraged = true;
                    }

                    // Evaluate health-based phase scaling
                    CheckPhaseScaling();
                }

                if (_def.AbilityRoar.Enabled && now >= _nextRoar)
                {
                    DoRoar();
                    _nextRoar = now + Mathf.Max(5f, _def.AbilityRoar.Interval);
                }

                if (_def.AbilityCharge.Enabled && now >= _nextCharge)
                {
                    DoCharge();
                    _nextCharge = now + Mathf.Max(8f, _def.AbilityCharge.Interval);
                }

                if (_def.AbilityFrostAura.Enabled && now >= _nextFrost)
                {
                    DoFrostAura();
                    _nextFrost = now + Mathf.Max(8f, _def.AbilityFrostAura.Interval);
                }

                if (_def.AbilityFireTrail.Enabled && now >= _nextFireTrail)
                {
                    DoFireTrail();
                    _nextFireTrail = now + Mathf.Max(8f, _def.AbilityFireTrail.Interval);
                }
            }

            #region Abilities

            private void CheckPhaseScaling()
            {
                if (_combat == null || _def.PhaseScales == null || _def.PhaseScales.Count == 0)
                    return;

                // If phase scaling is disabled in config, do nothing.
                if (!_plugin._config.UsePhaseScaling)
                    return;

                float currentHealth = _combat.health;
                float maxHealth = Mathf.Max(1f, _def.BaseHealth);
                float fraction = currentHealth / maxHealth;

                // Iterate through configured phases; trigger any whose HealthFraction
                // is >= fraction and not yet triggered.
                foreach (var phase in _def.PhaseScales)
                {
                    if (phase == null)
                        continue;

                    // Skip phases with obviously invalid or tiny scales.
                    if (phase.Scale <= 0f || float.IsNaN(phase.Scale) || float.IsInfinity(phase.Scale))
                        continue;

                    // Clamp threshold to 0–1.
                    var threshold = Mathf.Clamp01(phase.HealthFraction);

                    if (fraction <= threshold && !_triggeredPhaseScales.Contains(threshold))
                    {
                        _triggeredPhaseScales.Add(threshold);
                        _plugin.ScaleBeastEntity(_entity, phase.Scale);
                    }
                }
            }

            private void DoRoar()
            {
                var s = _def.AbilityRoar;
                var origin = _entity.transform.position + Vector3.up * 0.8f;

                // Pick a random roar FX (or fallback)
                var roarFx = _plugin.GetRandomFx("roar_blast", "assets/bundled/prefabs/fx/gestures/howl.prefab", _def.Theme);
                Effect.server.Run(roarFx, origin);

                var players = GetPlayersInRange(origin, s.Radius);
                foreach (var p in players)
                {
                    if (!p.IsAlive() || p.IsSleeping()) continue;
                    if (!HasLineOfSight(origin, p)) continue;

                    p.Hurt(s.Damage, DamageType.Slash, _entity, useProtection: false);
                    p.metabolism.bleeding.Add(s.Bleed);

                    p.SendConsoleCommand("gametip.showtoast", "warning", "A terrifying roar rattles you!");

                    // Optional impact FX at player
                    var impactFx = _plugin.GetRandomFx("ground_impact", "assets/bundled/prefabs/fx/impact/impact_concrete.prefab", _def.Theme);
                    Effect.server.Run(impactFx, p.transform.position);
                }
            }

            private void DoCharge()
            {
                var s = _def.AbilityCharge;
                var origin = _entity.transform.position;

                var target = FindBestTarget(origin, s.Range);
                if (target == null) return;

                var rb = EnsureRigidbody(_entity);
                var dir = (target.transform.position - origin);
                dir.y = 0f;
                dir = dir.normalized;

                rb.velocity = Vector3.zero;
                rb.AddForce(dir * s.ChargeForce, ForceMode.VelocityChange);

                Effect.server.Run("assets/bundled/prefabs/fx/gestures/flex.prefab", origin);

                _plugin.timer.Once(0.7f, () =>
                {
                    if (_entity == null || _entity.IsDestroyed) return;

                    var center = _entity.transform.position;
                    var players = GetPlayersInRange(center, s.ImpactRadius);
                    foreach (var p in players)
                    {
                        if (!p.IsAlive() || p.IsSleeping()) continue;
                        p.Hurt(s.ImpactDamage, DamageType.Blunt, _entity, useProtection: false);
                        Effect.server.Run("assets/bundled/prefabs/fx/impact/impact_ground.prefab", p.transform.position);
                    }
                });
            }

            private void DoFrostAura()
            {
                var s = _def.AbilityFrostAura;
                var ticks = Mathf.Max(1, Mathf.RoundToInt(s.Duration / Mathf.Max(0.1f, s.TickRate)));
                var tickInterval = Mathf.Max(0.1f, s.TickRate);

                var origin = _entity.transform.position + Vector3.up * 0.5f;
                var castFx = _plugin.GetRandomFx("frost_aura", "assets/bundled/prefabs/fx/hold_breath.prefab", _def.Theme);
                Effect.server.Run(castFx, origin);

                _plugin.timer.Repeat(tickInterval, ticks, () =>
                {
                    if (_entity == null || _entity.IsDestroyed) return;

                    var pos = _entity.transform.position + Vector3.up * 0.5f;
                    var players = GetPlayersInRange(pos, s.Radius);

                    foreach (var p in players)
                    {
                        if (!p.IsAlive() || p.IsSleeping()) continue;

                        p.Hurt(s.DamagePerTick, DamageType.Cold, _entity, useProtection: false);
                        p.metabolism.temperature.MoveTowards(0f, s.ColdPerTick);

                        p.SendConsoleCommand("gametip.showtoast", "warning", "You are chilled by a frost aura!");
                    }

                    var tickFx = _plugin.GetRandomFx("frost_aura", castFx, _def.Theme);
                    Effect.server.Run(tickFx, pos);
                });
            }

            private void DoCubSummon()
            {
                var s = _def.AbilityCubSummon;
                if (s.Count <= 0 || string.IsNullOrEmpty(s.Prefab)) return;

                var origin = _entity.transform.position;
                for (int i = 0; i < s.Count; i++)
                {
                    Vector3 spawnPos = origin + UnityEngine.Random.insideUnitSphere * s.Radius;
                    spawnPos.y = TerrainMeta.HeightMap.GetHeight(spawnPos);

                    var child = GameManager.server.CreateEntity(s.Prefab, spawnPos, Quaternion.identity, true);
                    if (child == null) continue;

                    child.enableSaving = false;
                    child.Spawn();

                    var childCombat = child as BaseCombatEntity;
                    if (childCombat != null)
                    {
                        childCombat.InitializeHealth(_def.BaseHealth * 0.25f, _def.BaseHealth * 0.25f);
                    }

                    Effect.server.Run(
                        _plugin.GetRandomFx("summon_burst", "assets/bundled/prefabs/fx/takeloot.prefab", _def.Theme),
                        spawnPos
                    );
                }

                _plugin.PrintBossChat($"<color=#ffb700>{_def.DisplayName}</color> summons allies to its side!");
            }

            private void TriggerEnrage()
            {
                var s = _def.AbilityEnrage;

                _plugin.ApplyEnrageBuff(_entity, _def);

                // Apply enraged scale if configured.
                _plugin.ScaleBeastEntity(_entity, _def.EnragedScale);

                var pos = _entity.transform.position + Vector3.up * 0.5f;

                var fx = _plugin.GetRandomFx("enrage_burst", s.EffectPrefab, _def.Theme);
                if (!string.IsNullOrEmpty(fx))
                    Effect.server.Run(fx, pos);

                _plugin.PrintBossChat($"<color=#ff0000>{_def.DisplayName}</color> becomes enraged!");
            }

            private void DoFireTrail()
            {
                var s = _def.AbilityFireTrail;
                var steps = Mathf.Max(1, Mathf.RoundToInt(s.Duration / Mathf.Max(0.1f, s.Step)));
                var interval = Mathf.Max(0.1f, s.Step);

                _plugin.timer.Repeat(interval, steps, () =>
                {
                    if (_entity == null || _entity.IsDestroyed) return;

                    var pos = _entity.transform.position;

                    var trailFx = _plugin.GetRandomFx("fire_trail", "assets/bundled/prefabs/fireball_small.prefab", _def.Theme);
                    Effect.server.Run(trailFx, pos);

                    var players = GetPlayersInRange(pos, s.Radius);
                    foreach (var p in players)
                    {
                        if (!p.IsAlive() || p.IsSleeping()) continue;
                        p.Hurt(s.DamagePerStep, DamageType.Heat, _entity, useProtection: false);
                        p.SendConsoleCommand("gametip.showtoast", "warning", "You are burned by searing flames!");
                    }
                });
            }

            #endregion

            #region Utility

            private static List<BasePlayer> GetPlayersInRange(Vector3 pos, float range)
            {
                var list = new List<BasePlayer>();
                foreach (var p in BasePlayer.activePlayerList)
                {
                    if (p == null || !p.IsConnected) continue;
                    if (Vector3.Distance(pos, p.transform.position) <= range)
                        list.Add(p);
                }
                return list;
            }

            private static BasePlayer FindBestTarget(Vector3 origin, float range)
            {
                BasePlayer best = null;
                var bestDist = float.MaxValue;

                foreach (var p in BasePlayer.activePlayerList)
                {
                    if (!p.IsAlive() || p.IsSleeping()) continue;
                    var d = Vector3.Distance(origin, p.transform.position);
                    if (d <= range && d < bestDist)
                    {
                        bestDist = d;
                        best = p;
                    }
                }

                return best;
            }

            private static bool HasLineOfSight(Vector3 from, BasePlayer target)
            {
                RaycastHit hit;
                if (!Physics.Raycast(from, (target.eyes.position - from).normalized, out hit, 999f,
                        Layers.Mask.Default | Layers.Mask.World))
                    return true;

                return hit.GetEntity() == target;
            }

            private static Rigidbody EnsureRigidbody(BaseEntity ent)
            {
                var rb = ent.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = ent.gameObject.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    rb.mass = 100f;
                }

                return rb;
            }

            #endregion
        }

        #endregion
    }
}
