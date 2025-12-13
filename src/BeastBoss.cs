using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal ConfigData _config;

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
                        EffectPrefab = null
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
                        EffectPrefab = null
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
                },

                ["storm_wolf"] = new BeastDef
                {
                    DisplayName = "Stormfang Alpha",
                    TierId = "T2",
                    Theme = "storm",
                    Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                    BaseHealth = 2200f,
                    DamageMultiplier = 1.45f,
                    InitialScale = 1.0f,
                    EnragedScale = 1.0f,

                    PhaseScales = new List<PhaseScale>
                    {
                        new PhaseScale { HealthFraction = 0.50f, Scale = 1.0f },
                        new PhaseScale { HealthFraction = 0.25f, Scale = 1.0f }
                    },

                    AbilityRoar = new RoarSettings
                    {
                        Enabled = true,
                        Interval = 12f,
                        Radius = 10f,
                        Damage = 9f,
                        Bleed = 6f,
                        ScreenShake = 1.6f
                    },

                    AbilityCharge = new ChargeSettings
                    {
                        Enabled = true,
                        Interval = 14f,
                        Range = 26f,
                        ImpactRadius = 2.8f,
                        ImpactDamage = 38f,
                        ChargeForce = 16f
                    },

                    AbilityFrostAura = new FrostAuraSettings
                    {
                        Enabled = false,
                        Interval = 16f,
                        Radius = 9f,
                        Duration = 4f,
                        TickRate = 1f,
                        DamagePerTick = 0f,
                        ColdPerTick = 0f
                    },

                    AbilityCubSummon = new CubSummonSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.45f,
                        Count = 2,
                        Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                        Radius = 8f
                    },

                    AbilityEnrage = new EnrageSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.30f,
                        DamageMultiplier = 1.8f,
                        SpeedMultiplier = 1.55f,
                        EffectPrefab = null
                    },

                    AbilityFireTrail = new FireTrailSettings
                    {
                        Enabled = false,
                        Interval = 18f,
                        Duration = 4f,
                        Step = 0.7f,
                        Radius = 3f,
                        DamagePerStep = 0f
                    },

                    Loot = new List<LootEntry>
                    {
                        new LootEntry { ShortName = "scrap", Amount = 180, Skin = 0 },
                        new LootEntry { ShortName = "metal.fragments", Amount = 350, Skin = 0 },
                        new LootEntry { ShortName = "wolfmeat", Amount = 15, Skin = 0 }
                    }
                },

                ["plague_boar"] = new BeastDef
                {
                    DisplayName = "Plague Tusk",
                    TierId = "T2",
                    Theme = "toxic",
                    Prefab = "assets/rust.ai/agents/boar/boar.prefab",
                    BaseHealth = 2000f,
                    DamageMultiplier = 1.35f,
                    InitialScale = 1.0f,
                    EnragedScale = 1.0f,

                    PhaseScales = new List<PhaseScale>
                    {
                        new PhaseScale { HealthFraction = 0.60f, Scale = 1.0f },
                        new PhaseScale { HealthFraction = 0.30f, Scale = 1.0f }
                    },

                    AbilityRoar = new RoarSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Radius = 9f,
                        Damage = 8f,
                        Bleed = 8f,
                        ScreenShake = 1.2f
                    },

                    AbilityCharge = new ChargeSettings
                    {
                        Enabled = true,
                        Interval = 18f,
                        Range = 22f,
                        ImpactRadius = 3.2f,
                        ImpactDamage = 34f,
                        ChargeForce = 14f
                    },

                    AbilityFrostAura = new FrostAuraSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Radius = 10f,
                        Duration = 5f,
                        TickRate = 1f,
                        DamagePerTick = 6f,
                        ColdPerTick = 0f
                    },

                    AbilityCubSummon = new CubSummonSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.50f,
                        Count = 2,
                        Prefab = "assets/rust.ai/agents/boar/boar.prefab",
                        Radius = 7f
                    },

                    AbilityEnrage = new EnrageSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.28f,
                        DamageMultiplier = 1.75f,
                        SpeedMultiplier = 1.45f,
                        EffectPrefab = null
                    },

                    AbilityFireTrail = new FireTrailSettings
                    {
                        Enabled = false,
                        Interval = 18f,
                        Duration = 4f,
                        Step = 0.7f,
                        Radius = 3f,
                        DamagePerStep = 0f
                    },

                    Loot = new List<LootEntry>
                    {
                        new LootEntry { ShortName = "scrap", Amount = 150, Skin = 0 },
                        new LootEntry { ShortName = "leather", Amount = 140, Skin = 0 },
                        new LootEntry { ShortName = "boarmeat", Amount = 18, Skin = 0 }
                    }
                },

                ["shade_stag"] = new BeastDef
                {
                    DisplayName = "Shadehorn",
                    TierId = "T2",
                    Theme = "shadow",
                    Prefab = "assets/rust.ai/agents/stag/stag.prefab",
                    BaseHealth = 1800f,
                    DamageMultiplier = 1.25f,
                    InitialScale = 1.0f,
                    EnragedScale = 1.0f,

                    PhaseScales = new List<PhaseScale>
                    {
                        new PhaseScale { HealthFraction = 0.50f, Scale = 1.0f },
                        new PhaseScale { HealthFraction = 0.20f, Scale = 1.0f }
                    },

                    AbilityRoar = new RoarSettings
                    {
                        Enabled = true,
                        Interval = 18f,
                        Radius = 11f,
                        Damage = 6f,
                        Bleed = 10f,
                        ScreenShake = 1.8f
                    },

                    AbilityCharge = new ChargeSettings
                    {
                        Enabled = true,
                        Interval = 16f,
                        Range = 24f,
                        ImpactRadius = 3.0f,
                        ImpactDamage = 28f,
                        ChargeForce = 15f
                    },

                    AbilityFrostAura = new FrostAuraSettings
                    {
                        Enabled = false,
                        Interval = 16f,
                        Radius = 9f,
                        Duration = 4f,
                        TickRate = 1f,
                        DamagePerTick = 0f,
                        ColdPerTick = 0f
                    },

                    AbilityCubSummon = new CubSummonSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.45f,
                        Count = 3,
                        Prefab = "assets/rust.ai/agents/wolf/wolf.prefab",
                        Radius = 9f
                    },

                    AbilityEnrage = new EnrageSettings
                    {
                        Enabled = true,
                        TriggerHealthFraction = 0.22f,
                        DamageMultiplier = 1.6f,
                        SpeedMultiplier = 1.6f,
                        EffectPrefab = null
                    },

                    AbilityFireTrail = new FireTrailSettings
                    {
                        Enabled = false,
                        Interval = 18f,
                        Duration = 4f,
                        Step = 0.7f,
                        Radius = 3f,
                        DamagePerStep = 0f
                    },

                    Loot = new List<LootEntry>
                    {
                        new LootEntry { ShortName = "scrap", Amount = 130, Skin = 0 },
                        new LootEntry { ShortName = "cloth", Amount = 120, Skin = 0 },
                        new LootEntry { ShortName = "deermeat.raw", Amount = 14, Skin = 0 }
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

            // Enable entity scaling via EntityScaleManager plugin.
            // If false, no scaling occurs. If true, requires EntityScaleManager to be installed.
            public bool UseScaling = false;

            // Enable phase-based scaling (beasts scale as health drops to phases).
            // If false, only InitialScale and EnragedScale are applied.
            public bool UsePhaseScaling = false;

            // Enable debug logging for boss lifecycle events (spawn, enrage, summon, etc).
            public bool Debug = false;

            // Boss HUD config (roughly compatible look with TargetHealthHUD)
            public UiConfig Ui = new UiConfig();

            // Chat announcements on boss spawn/enrage/death
            public AnnouncementSettings Announcements = new AnnouncementSettings();

            // World map markers that follow active bosses
            public MarkerSettings Markers = new MarkerSettings();

            // Proximity-based warnings to nearby players
            public ProximityWarningSettings ProximityWarnings = new ProximityWarningSettings();

            // Boss leash system: soft reset when kiting too far
            public LeashSettings Leash = new LeashSettings();

            // Weather-enhanced ability procs (lightning strikes, etc.)
            public WeatherProcSettings WeatherProcs = new WeatherProcSettings();

            // Timed world events that spawn bosses automatically
            public WorldEventSettings WorldEvents = new WorldEventSettings();

            // Per-player tier escalation: each player progresses through tiers based on their kill counts
            public PlayerTierEscalationSettings TierEscalation = new PlayerTierEscalationSettings();

            // Mythic variants: rare boss versions with special FX and loot
            public MythicSettings Mythic = new MythicSettings();

            // Boss title plate (CUI overlay at top-center showing boss name + subtitle)
            public TitlePlateSettings TitlePlate = new TitlePlateSettings();

            // Enrage countdown indicator shown in title plate
            public EnrageIndicatorSettings EnrageIndicator = new EnrageIndicatorSettings();

            /*
             * FX Library keys and usage:
             *  - "roar_blast"   : Used by DoRoar() for the main roar visual + optional impact FX.
             *  - "fire_trail"   : Used by DoFireTrail() for the trail FX behind fire-themed beasts.
             *  - "frost_aura"   : Used by DoFrostAura() as the primary cold aura visual.
             *  - "toxic_aura"   : Optional alternative to frost_aura for poison-themed beasts.
             *  - "ground_impact": Used by DoRoar() and other impact-style abilities for ground shock FX.
             *  - "enrage_burst" : Used by TriggerEnrage() for the enrage burst visual.
             *  - "enrage_aura"  : Used by TriggerEnrage() for the repeating aura during enrage.
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
                    "assets/bundled/prefabs/fx/player/howl.prefab",
                    "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab"
                },
                ["fire_trail"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/fire/fire_v2.prefab",
                    "assets/bundled/prefabs/fx/fire/fire_v3.prefab"
                },
                ["frost_aura"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/smoke/generator_smoke.prefab"
                },
                ["toxic_aura"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/smoke_cover_full.prefab"
                },
                ["ground_impact"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/explosions/explosion_01.prefab",
                    "assets/bundled/prefabs/fx/impacts/blunt/dirt/dirt1.prefab"
                },
                ["enrage_burst"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/explosions/explosion_core_flash.prefab",
                    "assets/bundled/prefabs/fx/explosions/explosion_02.prefab"
                },
                ["enrage_aura"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/fire/fire_v2.prefab",
                    "assets/bundled/prefabs/fx/smoke_signal.prefab"
                },
                ["summon_burst"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/impacts/additive/fire.prefab",
                    "assets/bundled/prefabs/fx/explosions/explosion_01.prefab"
                },
                ["lightning_strike"] = new List<string>
                {
                    "assets/bundled/prefabs/fx/player/electrocute.prefab",
                    "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab"
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

        public class AnnouncementSettings
        {
            public bool Enabled = true;
            public bool AnnounceSpawn = true;
            public bool AnnounceEnrage = true;
            public bool AnnounceDeath = true;

            public string SpawnMessage = "<color=#ffcc00>[BeastBoss]</color> <color=#ff5555>{name}</color> has appeared at <color=#aaddff>{grid}</color>!";
            public string EnrageMessage = "<color=#ffcc00>[BeastBoss]</color> <color=#ff5555>{name}</color> is <color=#ff0000>ENRAGED</color>!";
            public string DeathMessage = "<color=#ffcc00>[BeastBoss]</color> <color=#ff5555>{name}</color> was defeated by <color=#aaffaa>{killer}</color>!";
        }

        public class MarkerSettings
        {
            public bool Enabled = true;
            public float Radius = 0.25f;
            public float Alpha = 0.65f;
            public string Color = "#ff4444";
            public bool ShowLabel = true;
            public float UpdateIntervalSeconds = 3f;
        }

        public class ProximityWarningSettings
        {
            public bool Enabled = true;
            public float RadiusMeters = 120f;

            public bool WarnOnSpawn = true;
            public bool WarnOnEnrage = true;
            public bool WarnOnDeath = false;

            public string SpawnMessage = "<color=#ffcc00>[Warning]</color> A BeastBoss (<color=#ff5555>{name}</color>) has spawned nearby!";
            public string EnrageMessage = "<color=#ffcc00>[Warning]</color> <color=#ff5555>{name}</color> is <color=#ff0000>ENRAGED</color> nearby!";
            public string DeathMessage = "<color=#ffcc00>[Info]</color> Nearby BeastBoss (<color=#ff5555>{name}</color>) has been defeated.";

            public float PlayerCooldownSeconds = 20f;
        }

        public class LeashSettings
        {
            public bool Enabled = true;

            // Primary leash radius from the boss spawn point (meters).
            public float RadiusMeters = 80f;

            // If boss exceeds this distance, it resets immediately (safety mechanism).
            // Set to 0 to disable. Recommended: RadiusMeters * 1.25
            public float HardResetRadiusMeters = 120f;

            // If boss remains outside RadiusMeters for this long (seconds), it will reset.
            public float ResetAfterSecondsOutside = 8f;

            // Heal fraction on reset (1.0 = full heal).
            public float ResetHealFraction = 1.0f;

            // Incoming damage multiplier when boss is outside RadiusMeters.
            // Example: 0.25 reduces player damage to 25% while boss is outside leash.
            public float OutsideIncomingDamageMultiplier = 0.25f;

            // Return-to-spawn walk behavior
            public bool WalkBackToSpawn = true;
            public float ReturnStopDistanceMeters = 6f;     // how close is "back home"
            public float ReturnMaxSeconds = 30f;            // fail-safe: if stuck too long, fallback to teleport OR force-reset in place
            public bool FallbackTeleportIfStuck = true;     // if true, teleport to spawn after ReturnMaxSeconds
            public float ReturnDestinationRefreshSeconds = 2f; // how often to re-issue destination while returning

            // Optional messaging on reset
            public bool AnnounceResetToChat = true;
            public string ResetMessage = "<color=#ffcc00>[BeastBoss]</color> <color=#ff5555>{name}</color> has retreated and recovered!";
        }

        public class WeatherProcSettings
        {
            public bool Enabled = true;
            public bool StormThemeOnly = true;
            public bool RequireBadWeather = false; // if true, only proc during rain/fog
            public float ProcChancePerCheck = 0.15f;
            public float ProcCheckIntervalSeconds = 3f;
            public float ProcRadiusMeters = 4f;
            public float ProcDamage = 12f; // set 0 for FX only
        }

        public class WorldEventSettings
        {
            public bool Enabled = false;

            // Example: every 60–90 minutes, pick one boss and spawn it at a spawnpoint group.
            public float MinMinutesBetweenEvents = 60f;
            public float MaxMinutesBetweenEvents = 90f;

            public string SpawnpointGroup = "default"; // reuse your BotReSpawn-like manual spawnpoint grouping
            public List<string> AllowedBeastKeys = new List<string>(); // empty => all
        }

        public class PlayerTierEscalationSettings
        {
            public bool Enabled = true;
            public float ResetAfterHours = 0f; // 0 = disabled
            public bool CycleEnabled = false;
            public string CycleAtTierId = "T3";
            public string CycleToTierId = "T1";
            public List<PlayerTierRule> Rules = new List<PlayerTierRule>
            {
                new PlayerTierRule { FromTierId = "T1", KillsToAdvance = 10, ToTierId = "T2" },
                new PlayerTierRule { FromTierId = "T2", KillsToAdvance = 10, ToTierId = "T3" }
            };
            public bool KillerOnly = true;
        }

        public class PlayerTierRule
        {
            public string FromTierId;
            public int KillsToAdvance;
            public string ToTierId;
        }

        public class PlayerTierProgress
        {
            public string CurrentTierId = "T1";
            public Dictionary<string, int> KillsByTier = new Dictionary<string, int>();
            public double LastProgressUtc = 0;
        }

        public class MythicSettings
        {
            public bool Enabled = true;
            public float Chance = 0.05f;

            public string NamePrefix = "Mythic ";
            public string NameSuffix = "";
            public string ThemeOverride = ""; // optional, empty => keep original

            public string SpawnFxKey = "enrage_burst";
            public string EnrageFxKey = "enrage_aura";
            public string DeathFxKey = "explosion_big"; // if missing, reuse "enrage_burst"

            public float LootMultiplier = 1.5f; // multiply amounts; do not change items
        }

        public class TitlePlateSettings
        {
            public bool Enabled = true;
            public float ShowWithinMeters = 120f;
            public float UpdateIntervalSeconds = 1.0f;

            public string PanelAnchorMin = "0.30 0.93";
            public string PanelAnchorMax = "0.70 0.99";
            public string PanelColor = "0.05 0.05 0.05 0.65";

            public int FontSize = 18;
            public string TextColor = "1 1 1 1";
            public string OutlineColor = "0 0 0 0.8";

            // Tier-colored border configuration
            public float BorderThickness = 0.006f; // Anchor units, clamped 0.001-0.05
            public Dictionary<string, string> TierBorderColors = new Dictionary<string, string>
            {
                ["T1"] = "#ffcc00",  // Gold
                ["T2"] = "#66ccff",  // Cyan
                ["T3"] = "#cc66ff",  // Magenta
                ["T4"] = "#ff4444",  // Red
                ["T5"] = "#ffffff"   // White
            };

            // Mythic border overrides
            public string MythicBorderColor = "#ffd700";   // gold
            public bool MythicBorderOverridesTier = true;  // if true, use mythic color instead of tier color
            public bool MythicPulseOverrides = true;
            public float MythicPulseSpeedMultiplier = 1.35f; // pulse slightly faster than normal
            public float MythicPulseAlphaBoost = 0.08f;       // increases max alpha slightly (still subtle)

            // Title format tokens:
            // {name} -> boss display name (supports mythic override)
            // {subtitle} -> generated subtitle based on Theme/TierId
            public string TitleFormat = "{name}, {subtitle}";
        }

        public class EnrageIndicatorSettings
        {
            public bool Enabled = true;
            public string TextColor = "1 0.2 0.2 1";
            public int FontSize = 14;

            // position within TitlePlate panel
            public string AnchorMin = "0 0.05";
            public string AnchorMax = "1 0.45";
            public string Format = "ENRAGED: {seconds}s";

            // Pulsing effect on title plate when enraged
            public bool PulseTitlePlate = true;
            public float PulseMinAlpha = 0.45f;
            public float PulseMaxAlpha = 0.75f;
            public float PulseSpeed = 2.0f; // higher = faster pulse
            public bool PulseBorder = true;
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
            public string EffectPrefab = null;  // Falls back to FxLibrary "enrage_burst" if null
            public float Duration = 10f;  // Duration of enrage effect in seconds
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

        private class SpawnPoint
        {
            public Vector3Serializable Position;
            public float Weight = 1f;
        }

        private class StoredData
        {
            public Dictionary<string, List<Vector3Serializable>> TierSpawns =
                new Dictionary<string, List<Vector3Serializable>>();

            public Dictionary<string, List<SpawnPoint>> Spawnpoints =
                new Dictionary<string, List<SpawnPoint>>();

            public Dictionary<string, Dictionary<ulong, double>> TierLockouts =
                new Dictionary<string, Dictionary<ulong, double>>();

            // Per-player tier progression (replaces global TierKills)
            public Dictionary<ulong, PlayerTierProgress> PlayerProgress =
                new Dictionary<ulong, PlayerTierProgress>();
        }

        private StoredData _data;

        private const string ExternalHudPermission = "targethealthhud.use";

        // Players for whom we've temporarily revoked TargetHealthHUD permission
        private readonly HashSet<ulong> _suspendedExternalHud = new HashSet<ulong>();

        private readonly HashSet<BaseEntity> _bosses = new HashSet<BaseEntity>();
        private readonly Dictionary<ulong, float> _damageMeter = new Dictionary<ulong, float>();
        private readonly Dictionary<uint, BeastDef> _beastDefs = new Dictionary<uint, BeastDef>();
        private readonly Dictionary<uint, float> _bossDamageMultipliers = new Dictionary<uint, float>();

        private readonly Dictionary<string, BaseEntity> _activeBossByTier = new Dictionary<string, BaseEntity>();
        private readonly Dictionary<uint, string> _bossTierById = new Dictionary<uint, string>();
        private readonly Dictionary<string, double> _tierLastDeathTime = new Dictionary<string, double>();
        private readonly Dictionary<uint, MapMarkerGenericRadius> _bossMarkers =
            new Dictionary<uint, MapMarkerGenericRadius>();

        // HUD tracking: player -> boss & timers
        private readonly Dictionary<ulong, BaseCombatEntity> _hudBossTargets = new Dictionary<ulong, BaseCombatEntity>();
        private readonly Dictionary<ulong, Timer> _hudUpdateTimers = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> _hudExpireTimers = new Dictionary<ulong, Timer>();

        // Marker update timer and proximity warning cooldowns
        private Timer _markerUpdateTimer;
        private Timer _worldEventTimer;
        private readonly Dictionary<ulong, float> _proximityCooldown = new Dictionary<ulong, float>();

        // Boss component lookup for leash system and spawn point retrieval
        private readonly Dictionary<uint, BeastComponent> _bossComponents = new Dictionary<uint, BeastComponent>();

        // Mythic boss tracking
        private readonly HashSet<uint> _mythicBossIds = new HashSet<uint>();

        // Runtime name and theme overrides (for mythic variants)
        private readonly Dictionary<uint, string> _runtimeBossName = new Dictionary<uint, string>();
        private readonly Dictionary<uint, string> _runtimeBossTheme = new Dictionary<uint, string>();

        // Title plate (CUI overlay) timer
        private Timer _titlePlateTimer;

        private const string TitlePlateUi = "BeastBoss_TitlePlateUI";

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
            if (_data.Spawnpoints == null)
                _data.Spawnpoints = new Dictionary<string, List<SpawnPoint>>();
            if (_data.TierLockouts == null)
                _data.TierLockouts = new Dictionary<string, Dictionary<ulong, double>>();
            if (_data.PlayerProgress == null)
                _data.PlayerProgress = new Dictionary<ulong, PlayerTierProgress>();
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

            // Start marker update timer
            if (_markerUpdateTimer == null && _config.Markers.Enabled)
                _markerUpdateTimer = timer.Every(Mathf.Max(1f, _config.Markers.UpdateIntervalSeconds), UpdateBossMarkers);

            // Start title plate update timer
            if (_titlePlateTimer == null && _config.TitlePlate.Enabled)
                _titlePlateTimer = timer.Every(Mathf.Max(0.5f, _config.TitlePlate.UpdateIntervalSeconds), UpdateTitlePlates);

            // Schedule world events
            if (_config.WorldEvents.Enabled)
                ScheduleNextWorldEvent();
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
                DestroyTitlePlate(player);
            }

            _bosses.Clear();
            _beastDefs.Clear();
            _bossDamageMultipliers.Clear();
            _activeBossByTier.Clear();
            _bossTierById.Clear();
            _bossMarkers.Clear();
            _proximityCooldown.Clear();
            _bossComponents.Clear();
            _mythicBossIds.Clear();
            _runtimeBossName.Clear();
            _runtimeBossTheme.Clear();

            if (_markerUpdateTimer != null)
            {
                _markerUpdateTimer.Destroy();
                _markerUpdateTimer = null;
            }

            if (_titlePlateTimer != null)
            {
                _titlePlateTimer.Destroy();
                _titlePlateTimer = null;
            }

            if (_worldEventTimer != null)
            {
                _worldEventTimer.Destroy();
                _worldEventTimer = null;
            }

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

        private string GetMythicDisplayName(BeastDef def)
        {
            return _config.Mythic.NamePrefix + def.DisplayName + _config.Mythic.NameSuffix;
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;

            // Outgoing damage from boss
            if (info.Initiator != null && _bosses.Contains(info.Initiator))
            {
                uint initiatorId = NetId(info.Initiator);
                float mult = 1f;
                if (_bossDamageMultipliers.TryGetValue(initiatorId, out var m))
                    mult = m;

                info.damageTypes.ScaleAll(_config.GlobalOutgoingDamageMultiplier * mult);
            }

            // Incoming damage to boss
            if (_bosses.Contains(entity))
            {
                BeastDef def;
                uint bossId = NetId(entity);
                _beastDefs.TryGetValue(bossId, out def);
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

                // Apply damage reduction if boss is returning to spawn or outside leash radius
                if (_config.Leash.Enabled)
                {
                    BeastComponent comp;
                    if (_bossComponents.TryGetValue(bossId, out comp))
                    {
                        // Even stronger reduction while returning (to prevent players from intercepting)
                        if (comp.IsReturning)
                        {
                            info.damageTypes.ScaleAll(0.05f);
                        }
                        else if (_config.Leash.OutsideIncomingDamageMultiplier < 1f && IsBossOutsideLeash(entity, comp.SpawnPos))
                        {
                            info.damageTypes.ScaleAll(_config.Leash.OutsideIncomingDamageMultiplier);
                        }
                    }
                }

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
            uint bossId = NetId(entity);
            if (!_beastDefs.TryGetValue(bossId, out def)) return;

            string tierId = def.TierId;
            var now = Interface.Oxide.Now;

            // Check if this was a mythic variant
            bool isMythic = _mythicBossIds.Contains(bossId);

            // Get killer name for announcements
            BasePlayer killer = info?.InitiatorPlayer;
            string killerName = killer != null ? killer.displayName : "unknown";

            // Update per-player tier progression (only if there is a killer)
            if (!string.IsNullOrEmpty(tierId) && killer != null && _config.TierEscalation.Enabled)
            {
                var p = GetOrCreateProgress(killer.userID);
                ApplyProgressResetIfNeeded(p);

                // Increment kills for this tier
                p.KillsByTier[tierId] = p.KillsByTier.GetValueOrDefault(tierId, 0) + 1;
                p.LastProgressUtc = DateTime.UtcNow.ToOADate();

                int currentKills = p.KillsByTier[tierId];
                Dbg($"Player {killerName} ({killer.userID}): tier '{tierId}' kill count is now {currentKills}");

                // Check if player advances tier
                var rule = _config.TierEscalation.Rules?.FirstOrDefault(r =>
                    string.Equals(r.FromTierId, p.CurrentTierId, StringComparison.OrdinalIgnoreCase));

                if (rule != null && string.Equals(tierId, p.CurrentTierId, StringComparison.OrdinalIgnoreCase) &&
                    currentKills >= rule.KillsToAdvance)
                {
                    string prevTier = p.CurrentTierId;
                    p.CurrentTierId = ResolveNextTierId(p.CurrentTierId);
                    p.KillsByTier[prevTier] = 0; // Reset kill count for previous tier
                    Dbg($"Player {killerName} advanced from tier '{prevTier}' to '{p.CurrentTierId}'");
                }

                SaveData();
            }

            // Mythic death FX with runtime theme
            if (isMythic)
            {
                var theme = GetBossTheme(bossId, def.Theme);
                var key = string.IsNullOrEmpty(_config.Mythic.DeathFxKey) ? "explosion_big" : _config.Mythic.DeathFxKey;
                var mythicDeathFx = GetRandomFx(key, null, theme);
                if (!string.IsNullOrEmpty(mythicDeathFx))
                {
                    Effect.server.Run(mythicDeathFx, entity.transform.position + Vector3.up * 0.5f);
                }
                _mythicBossIds.Remove(bossId);
                _runtimeBossName.Remove(bossId);
                _runtimeBossTheme.Remove(bossId);
                Dbg($"Mythic boss death FX triggered and cleared from tracking");
            }

            // Remove world map marker
            RemoveBossMarker(entity);

            if (!string.IsNullOrEmpty(tierId))
            {
                if (_activeBossByTier.TryGetValue(tierId, out var existing) && existing == entity)
                    _activeBossByTier.Remove(tierId);

                _tierLastDeathTime[tierId] = now;
            }

            // Announcement on death
            if (_config.Announcements.Enabled && _config.Announcements.AnnounceDeath)
            {
                var deathMsg = FormatMessage(_config.Announcements.DeathMessage, def, entity, killerName);
                AnnounceToChat(deathMsg);
            }

            // Proximity warning on death
            if (_config.ProximityWarnings.Enabled && _config.ProximityWarnings.WarnOnDeath)
            {
                var warnMsg = FormatMessage(_config.ProximityWarnings.DeathMessage, def, entity, killerName);
                WarnPlayersNear(entity.transform.position, _config.ProximityWarnings.RadiusMeters, warnMsg);
            }

            // Use runtime display name (includes mythic variant names)
            var deathDisplayName = GetBossDisplayName(bossId, def.DisplayName);
            AnnounceNearby(entity.transform.position, $"{deathDisplayName} has been slain!");
            DropConfiguredLoot(bossId, entity.transform.position, def);

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
            _beastDefs.Remove(bossId);
            _bossDamageMultipliers.Remove(bossId);
            _bossComponents.Remove(bossId);  // Clean up component tracking

            if (_bossMarkers.TryGetValue(bossId, out var marker) && marker != null && !marker.IsDestroyed)
            {
                marker.Kill();
            }
            _bossMarkers.Remove(bossId);
            _bossTierById.Remove(bossId);

            // Clear HUD for any players tracking this boss
            ClearHudForBoss(entity);

            SaveData();
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            DestroyTitlePlate(player);
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

        [ChatCommand("beastaddspawn")]
        private void CmdBeastAddSpawn(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            string group = args.Length > 0 ? args[0] : "default";
            var pos = player.transform.position;

            if (!_data.Spawnpoints.TryGetValue(group, out var spawnList))
            {
                spawnList = new List<SpawnPoint>();
                _data.Spawnpoints[group] = spawnList;
            }

            var sp = new SpawnPoint
            {
                Position = Vector3Serializable.FromVector3(pos),
                Weight = 1f
            };

            spawnList.Add(sp);
            SaveData();

            SendReply(player, $"{_config.ChatPrefix}Added spawnpoint to group '{group}' at {pos} with weight 1.0 (index: {spawnList.Count - 1})");
        }

        [ChatCommand("beastremovespawn")]
        private void CmdBeastRemoveSpawn(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (args.Length < 2)
            {
                SendReply(player, $"{_config.ChatPrefix}Usage: /beastremovespawn <group> <index>");
                return;
            }

            string group = args[0];
            if (!int.TryParse(args[1], out int index))
            {
                SendReply(player, $"{_config.ChatPrefix}Invalid index.");
                return;
            }

            if (!_data.Spawnpoints.TryGetValue(group, out var spawnList) || index < 0 || index >= spawnList.Count)
            {
                SendReply(player, $"{_config.ChatPrefix}Group '{group}' or index {index} not found.");
                return;
            }

            spawnList.RemoveAt(index);
            SaveData();

            SendReply(player, $"{_config.ChatPrefix}Removed spawnpoint {index} from group '{group}'.");
        }

        [ChatCommand("beastspweight")]
        private void CmdBeastSpawnWeight(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (args.Length < 3)
            {
                SendReply(player, $"{_config.ChatPrefix}Usage: /beastspweight <group> <index> <weight>");
                return;
            }

            string group = args[0];
            if (!int.TryParse(args[1], out int index) || !float.TryParse(args[2], out float weight))
            {
                SendReply(player, $"{_config.ChatPrefix}Invalid index or weight.");
                return;
            }

            if (weight <= 0f)
            {
                SendReply(player, $"{_config.ChatPrefix}Weight must be greater than 0.");
                return;
            }

            if (!_data.Spawnpoints.TryGetValue(group, out var spawnList) || index < 0 || index >= spawnList.Count)
            {
                SendReply(player, $"{_config.ChatPrefix}Group '{group}' or index {index} not found.");
                return;
            }

            spawnList[index].Weight = weight;
            SaveData();

            SendReply(player, $"{_config.ChatPrefix}Set weight of spawnpoint {index} in group '{group}' to {weight}.");
        }

        [ChatCommand("beastlistspawns")]
        private void CmdBeastListSpawns(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _config.PermissionAdmin))
            {
                SendReply(player, $"{_config.ChatPrefix}You lack permission.");
                return;
            }

            if (_data.Spawnpoints.Count == 0)
            {
                SendReply(player, $"{_config.ChatPrefix}No spawnpoint groups defined.");
                return;
            }

            SendReply(player, $"{_config.ChatPrefix}Spawnpoint groups:");
            foreach (var kv in _data.Spawnpoints)
            {
                SendReply(player, $"- {kv.Key}: {kv.Value.Count} point(s)");
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    var sp = kv.Value[i];
                    var pos = sp.Position.ToVector3();
                    SendReply(player, $"  [{i}] {pos} weight={sp.Weight}");
                }
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

        private string GetGrid(Vector3 pos)
        {
            // Best-effort grid conversion using TerrainMeta + ConVar.Server.worldsize
            // Returns like "H12" or "Unknown" if we can't calculate.
            try
            {
                float worldSize = 0f;
                try { worldSize = ConVar.Server.worldsize; } catch { worldSize = 0f; }

                if (worldSize <= 0f)
                {
                    // Fallback to TerrainMeta if server var isn't available
                    try
                    {
                        if (TerrainMeta.Size.x > 0f) worldSize = TerrainMeta.Size.x;
                    }
                    catch { }
                }

                if (worldSize <= 0f) return "Unknown";

                // Rust coordinates are centered at (0,0). Convert to 0..worldSize space.
                float half = worldSize * 0.5f;
                float x = pos.x + half;
                float z = half - pos.z; // invert Z so north is smaller number

                // clamp
                x = Mathf.Clamp(x, 0f, worldSize - 0.01f);
                z = Mathf.Clamp(z, 0f, worldSize - 0.01f);

                // Standard Rust grids are 26 columns (A-Z). Some servers exceed; we'll support AA, AB... too.
                const int cols = 26;
                float cellSize = worldSize / cols;

                int col = Mathf.FloorToInt(x / cellSize);
                int row = Mathf.FloorToInt(z / cellSize) + 1; // rows are 1-based

                string letters = ColToLetters(col);
                return $"{letters}{row}";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string ColToLetters(int col)
        {
            // 0 -> A, 25 -> Z, 26 -> AA, 27 -> AB ...
            col = Mathf.Max(0, col);
            string s = "";
            while (true)
            {
                int r = col % 26;
                s = ((char)('A' + r)) + s;
                col = (col / 26) - 1;
                if (col < 0) break;
            }
            return s;
        }

        private string FormatMessage(string template, BeastDef def, BaseEntity boss, string killerName = null)
        {
            if (template == null) return "";

            string message = template;
            message = message.Replace("{name}", def != null ? def.DisplayName : "Unknown");
            message = message.Replace("{grid}", boss != null ? GetGrid(boss.transform.position) : "unknown");
            message = message.Replace("{killer}", !string.IsNullOrEmpty(killerName) ? killerName : "unknown");
            return message;
        }

        private void AnnounceToChat(string message)
        {
            if (!_config.Announcements.Enabled) return;
            if (string.IsNullOrEmpty(message)) return;
            Server.Broadcast(message);
        }

        private void WarnPlayersNear(Vector3 pos, float radius, string message)
        {
            if (!_config.ProximityWarnings.Enabled) return;
            if (string.IsNullOrEmpty(message)) return;

            var now = Time.realtimeSinceStartup;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || !player.IsConnected) continue;

                float distance = Vector3.Distance(pos, player.transform.position);
                if (distance > radius) continue;

                // Check player cooldown
                float lastWarn;
                if (_proximityCooldown.TryGetValue(player.userID, out lastWarn))
                {
                    if (now - lastWarn < _config.ProximityWarnings.PlayerCooldownSeconds)
                        continue;
                }

                _proximityCooldown[player.userID] = now;
                player.ChatMessage(message);
            }
        }

        private void CreateBossMarker(BaseEntity boss, BeastDef def)
        {
            if (!_config.Markers.Enabled || boss == null || boss.IsDestroyed) return;

            RemoveBossMarker(boss);

            var marker = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", boss.transform.position, Quaternion.identity);
            if (marker == null) return;

            marker.Spawn();

            try
            {
                var radiusMarker = marker as MapMarkerGenericRadius;
                if (radiusMarker != null)
                {
                    radiusMarker.alpha = _config.Markers.Alpha;
                    radiusMarker.color1 = ParseColor(_config.Markers.Color);
                    radiusMarker.radius = _config.Markers.Radius;
                    if (_config.Markers.ShowLabel && def != null)
                    {
                        // Use runtime display name (includes mythic variant names)
                        var displayName = GetBossDisplayName(NetId(boss), def.DisplayName);
                        TrySetMarkerLabel(radiusMarker, displayName);
                    }
                    radiusMarker.SendUpdate();
                }
            }
            catch { }

            _bossMarkers[NetId(boss)] = marker as MapMarkerGenericRadius;
        }

        private void RemoveBossMarker(BaseEntity boss)
        {
            if (boss == null || boss.net == null) return;

            MapMarkerGenericRadius marker;
            uint bossId = NetId(boss);
            if (_bossMarkers.TryGetValue(bossId, out marker))
            {
                _bossMarkers.Remove(bossId);
                if (marker != null && !marker.IsDestroyed) marker.Kill();
            }
        }

        private void TrySetMarkerLabel(BaseEntity markerEntity, string label)
        {
            if (markerEntity == null || string.IsNullOrEmpty(label)) return;

            try
            {
                // Try common string field/property names used on marker components.
                var t = markerEntity.GetType();

                // 1) property: "markerName"
                var p1 = t.GetProperty("markerName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p1 != null && p1.PropertyType == typeof(string) && p1.CanWrite)
                {
                    p1.SetValue(markerEntity, label, null);
                    markerEntity.SendNetworkUpdate();
                    return;
                }

                // 2) field: "markerName"
                var f1 = t.GetField("markerName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f1 != null && f1.FieldType == typeof(string))
                {
                    f1.SetValue(markerEntity, label);
                    markerEntity.SendNetworkUpdate();
                    return;
                }

                // 3) property: "text"
                var p2 = t.GetProperty("text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p2 != null && p2.PropertyType == typeof(string) && p2.CanWrite)
                {
                    p2.SetValue(markerEntity, label, null);
                    markerEntity.SendNetworkUpdate();
                    return;
                }

                // 4) method: SetLabel(string) if it exists in other builds
                var mi = t.GetMethod("SetLabel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (mi != null)
                {
                    mi.Invoke(markerEntity, new object[] { label });
                    markerEntity.SendNetworkUpdate();
                    return;
                }
            }
            catch
            {
                // ignore, label is optional
            }
        }

        private void UpdateBossMarkers()
        {
            if (!_config.Markers.Enabled) return;

            foreach (var boss in _bosses.ToArray())
            {
                if (boss == null || boss.IsDestroyed)
                    continue;

                MapMarkerGenericRadius marker;
                if (_bossMarkers.TryGetValue(NetId(boss), out marker))
                {
                    if (marker == null || marker.IsDestroyed) continue;
                    marker.transform.position = boss.transform.position;
                    marker.SendNetworkUpdate();
                }
            }

            // Clean up markers that no longer have an active boss
            foreach (var kvp in _bossMarkers.ToArray())
            {
                var bossId = kvp.Key;
                bool stillExists = _bosses.Any(b => b != null && !b.IsDestroyed && b.net != null && NetId(b) == bossId);
                if (!stillExists)
                {
                    var marker = kvp.Value;
                    _bossMarkers.Remove(bossId);
                    if (marker != null && !marker.IsDestroyed) marker.Kill();
                }
            }
        }

        private void UpdateTitlePlates()
        {
            if (_config == null || !_config.TitlePlate.Enabled) return;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || !player.IsConnected) continue;

                if (TryGetNearestBossForPlayer(player, out var boss, out var def))
                {
                    var title = BuildTitle(def, boss);
                    var bossId = boss.net?.ID.Value ?? 0u;

                    // Compute enrage countdown and pulse flag if applicable
                    string enrageText = null;
                    bool enraged = false;
                    if (_config.EnrageIndicator.Enabled && boss?.net != null)
                    {
                        // Get the BeastComponent to check enrage status
                        if (_bossComponents.TryGetValue(NetId(boss), out var comp) && comp != null)
                        {
                            if (comp.IsEnraged && comp.EnrageSecondsRemaining > 0f)
                            {
                                enraged = true;
                                int seconds = Mathf.CeilToInt(comp.EnrageSecondsRemaining);
                                enrageText = _config.EnrageIndicator.Format.Replace("{seconds}", seconds.ToString());
                            }
                        }
                    }

                    DrawTitlePlate(player, bossId, def, title, enrageText, enraged);
                }
                else
                {
                    DestroyTitlePlate(player);
                }
            }
        }

        private Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.red;
            Color c;
            if (ColorUtility.TryParseHtmlString(hex, out c)) return c;
            return Color.red;
        }

        // ==================== NETWORK ID HELPERS ====================
        // Normalize NetworkableId to uint for dictionary/set keys
        private uint NetId(BaseEntity ent)
        {
            if (ent == null || ent.net == null) return 0u;
            try { return ent.net.ID.Value; } catch { try { return (uint)ent.net.ID; } catch { return 0u; } }
        }

        private uint NetId(BaseNetworkable net)
        {
            if (net == null || net.net == null) return 0u;
            try { return net.net.ID.Value; } catch { try { return (uint)net.net.ID; } catch { return 0u; } }
        }
        // ==================== END NETWORK ID HELPERS ====================

        private NetworkableId ToNetId(uint id)
        {
            try { return new NetworkableId(id); } catch { return default(NetworkableId); }
        }

        private bool IsMythicBoss(uint id)
        {
            return _mythicBossIds != null && _mythicBossIds.Contains(id);
        }

        private string GetTierBorderHex(BeastDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.TierId)) return "#ffffff";
            string hex;
            if (_config.TitlePlate.TierBorderColors.TryGetValue(def.TierId, out hex))
                return hex;
            return "#ffffff"; // Default fallback
        }

        private string GetBorderHex(BeastDef def, uint id)
        {
            if (_config?.TitlePlate == null) return "#ffffff";

            bool mythic = IsMythicBoss(id);
            if (mythic && _config.TitlePlate.MythicBorderOverridesTier && !string.IsNullOrEmpty(_config.TitlePlate.MythicBorderColor))
                return _config.TitlePlate.MythicBorderColor;

            // Fall back to tier mapping
            return GetTierBorderHex(def);
        }

        private string ToCuiColor(Color c)
        {
            return $"{c.r} {c.g} {c.b} {c.a}";
        }

        private float GetPulseAlpha(bool mythic = false)
        {
            float speed = Mathf.Max(0.1f, _config.EnrageIndicator.PulseSpeed);
            float minA = _config.EnrageIndicator.PulseMinAlpha;
            float maxA = _config.EnrageIndicator.PulseMaxAlpha;

            if (mythic && _config.TitlePlate.MythicPulseOverrides)
            {
                speed *= Mathf.Max(1f, _config.TitlePlate.MythicPulseSpeedMultiplier);
                maxA = Mathf.Clamp01(maxA + Mathf.Max(0f, _config.TitlePlate.MythicPulseAlphaBoost));
            }

            var t = Time.realtimeSinceStartup * speed;
            var s = (Mathf.Sin(t) + 1f) * 0.5f;
            return Mathf.Lerp(minA, maxA, s);
        }

        private string WithAlpha(string rgba, float alpha)
        {
            if (string.IsNullOrEmpty(rgba)) return $"0 0 0 {alpha:0.###}";
            var parts = rgba.Split(' ');
            if (parts.Length < 4) return rgba;

            // Keep RGB, replace A
            return $"{parts[0]} {parts[1]} {parts[2]} {alpha:0.###}";
        }

        // ==================== PER-PLAYER TIER PROGRESSION ====================

        private PlayerTierProgress GetOrCreateProgress(ulong userId)
        {
            if (!_data.PlayerProgress.TryGetValue(userId, out var p) || p == null)
            {
                p = new PlayerTierProgress { CurrentTierId = "T1", LastProgressUtc = DateTime.UtcNow.ToOADate() };
                _data.PlayerProgress[userId] = p;
            }
            return p;
        }

        private void ApplyProgressResetIfNeeded(PlayerTierProgress p)
        {
            if (p == null) return;
            if (!_config.TierEscalation.Enabled) return;

            float hours = _config.TierEscalation.ResetAfterHours;
            if (hours <= 0f) return;

            double now = DateTime.UtcNow.ToOADate();
            double elapsedHours = (now - p.LastProgressUtc) * 24.0;
            if (elapsedHours >= hours)
            {
                p.CurrentTierId = "T1";
                p.KillsByTier.Clear();
                p.LastProgressUtc = now;
                Dbg($"Player tier progress reset (inactive for {elapsedHours:F1} hours)");
            }
        }

        private string ResolveNextTierId(string currentTierId)
        {
            if (!_config.TierEscalation.Enabled) return currentTierId;

            // Cycle logic first (if at cap tier)
            if (_config.TierEscalation.CycleEnabled &&
                string.Equals(currentTierId, _config.TierEscalation.CycleAtTierId, StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(_config.TierEscalation.CycleToTierId) ? "T1" : _config.TierEscalation.CycleToTierId;
            }

            // Normal rule-based progression
            var rules = _config.TierEscalation.Rules;
            if (rules != null)
            {
                foreach (var r in rules)
                {
                    if (string.Equals(r.FromTierId, currentTierId, StringComparison.OrdinalIgnoreCase))
                        return string.IsNullOrEmpty(r.ToTierId) ? currentTierId : r.ToTierId;
                }
            }

            return currentTierId; // no rule => stay
        }

        private string GetPlayerCurrentTier(ulong userId)
        {
            var p = GetOrCreateProgress(userId);
            ApplyProgressResetIfNeeded(p);
            if (string.IsNullOrEmpty(p.CurrentTierId)) p.CurrentTierId = "T1";
            return p.CurrentTierId;
        }

        // ==================== END PER-PLAYER TIER PROGRESSION ====================

        private bool IsBossOutsideLeash(BaseEntity boss, Vector3 spawnPos)
        {
            if (boss == null || boss.IsDestroyed || !_config.Leash.Enabled)
                return false;

            float r = _config.Leash.RadiusMeters;
            if (r <= 0f) return false;

            return Vector3.Distance(boss.transform.position, spawnPos) > r;
        }

        private bool IsBossBeyondHardReset(BaseEntity boss, Vector3 spawnPos)
        {
            if (boss == null || boss.IsDestroyed || !_config.Leash.Enabled)
                return false;

            float hr = _config.Leash.HardResetRadiusMeters;
            if (hr <= 0f) return false;

            return Vector3.Distance(boss.transform.position, spawnPos) > hr;
        }

        private bool TrySetNpcDestination(BaseNpc npc, Vector3 dest)
        {
            if (npc == null) return false;

            // 1) Direct call if method exists in this build
            try
            {
                var mi = npc.GetType().GetMethod("SetDestination", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Vector3) }, null);
                if (mi != null)
                {
                    mi.Invoke(npc, new object[] { dest });
                    return true;
                }
            }
            catch { }

            // 2) Try Navigator-style APIs via reflection (best-effort)
            try
            {
                var navProp = npc.GetType().GetProperty("Navigator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var navObj = navProp?.GetValue(npc, null);
                if (navObj != null)
                {
                    var mi2 = navObj.GetType().GetMethod("SetDestination", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Vector3) }, null);
                    if (mi2 != null)
                    {
                        mi2.Invoke(navObj, new object[] { dest });
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private void BeginReturnToSpawn(BeastComponent comp)
        {
            if (comp == null) return;

            var boss = comp.Entity;
            var def = comp.Def;
            var spawnPos = comp.SpawnPos;

            if (boss == null || boss.IsDestroyed) return;

            var npc = boss as BaseNpc;

            // Mark returning
            comp.SetReturning(true);

            // Clear aggro / chasing (best-effort)
            if (npc != null)
            {
                try { npc.SetFact(BaseNpc.Facts.IsAggro, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsChasing, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsAfraid, 0, true, true); } catch { }
            }

            // Issue destination to spawn
            if (npc != null)
                TrySetNpcDestination(npc, spawnPos);

            // Optional messaging
            if (_config.Leash.AnnounceResetToChat && _config.Announcements != null && _config.Announcements.Enabled)
            {
                Server.Broadcast(_config.Leash.ResetMessage.Replace("{name}", def.DisplayName));
            }

            Dbg($"Boss '{def.DisplayName}' starting return-to-spawn walk from {boss.transform.position} to {spawnPos}");
        }

        private void CompleteReturnReset(BaseEntity boss, BeastDef def)
        {
            if (boss == null || boss.IsDestroyed) return;

            var combat = boss as BaseCombatEntity;
            if (combat != null)
            {
                float max = Mathf.Max(1f, def.BaseHealth);
                float healTo = max * Mathf.Clamp01(_config.Leash.ResetHealFraction);
                combat.health = healTo;
                combat.SendNetworkUpdateImmediate();
            }

            // Attempt to calm the AI to a neutral state
            var npc = boss as BaseNpc;
            if (npc != null)
            {
                try { npc.SetFact(BaseNpc.Facts.IsAggro, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsAfraid, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsChasing, 0, true, true); } catch { }
            }

            Dbg($"Boss '{def.DisplayName}' completed return-to-spawn reset at spawn location");
        }

        private bool IsBadWeather()
        {
            try
            {
                // Try to check ConVar weather values
                float rain = ConVar.Weather.rain;
                float fog = ConVar.Weather.fog;
                return rain > 0.3f || fog > 0.3f;
            }
            catch
            {
                // Fallback: if RequireBadWeather is false, allow procs anyway; otherwise deny
                return !_config.WeatherProcs.RequireBadWeather;
            }
        }

        private bool TryGetNearestBossForPlayer(BasePlayer player, out BaseEntity boss, out BeastDef def)
        {
            boss = null;
            def = null;

            if (player == null) return false;
            if (_bossComponents == null || _bossComponents.Count == 0) return false;

            float maxDist = Mathf.Max(5f, _config.TitlePlate.ShowWithinMeters);
            float maxDistSqr = maxDist * maxDist;

            float best = float.MaxValue;

            foreach (var kvp in _bossComponents)
            {
                var comp = kvp.Value;
                if (comp == null) continue;

                var e = comp.Entity;
                if (e == null || e.IsDestroyed) continue;

                var d = comp.Def;
                if (d == null) continue;

                float ds = (player.transform.position - e.transform.position).sqrMagnitude;
                if (ds <= maxDistSqr && ds < best)
                {
                    best = ds;
                    boss = e;
                    def = d;
                }
            }

            return boss != null && def != null;
        }

        private void DoLightningProc(BaseEntity boss, BasePlayer target, float damage, float radius, string theme = "storm")
        {
            if (boss == null || target == null) return;

            // Pick position at target + small random offset within radius
            var targetPos = target.transform.position;
            var randomOffset = UnityEngine.Random.insideUnitSphere * radius;
            randomOffset.y = 0f; // Keep on ground plane
            var strikePos = targetPos + randomOffset;

            // Run lightning strike FX with runtime theme
            var fx = GetRandomFx("lightning_strike", null, theme);
            if (!string.IsNullOrEmpty(fx))
            {
                Effect.server.Run(fx, strikePos);
            }

            // Apply damage if configured
            if (damage > 0f)
            {
                try
                {
                    HitInfo info = new HitInfo(boss, target, DamageType.ElectricShock, damage);
                    target.Hurt(info);
                }
                catch
                {
                    // Fallback: direct damage method
                    target.Hurt(damage, DamageType.ElectricShock, boss, useProtection: false);
                }
            }

            Dbg($"Lightning proc triggered at {strikePos} targeting {target.displayName} for {damage} damage");
        }

        private BasePlayer GetPrimaryTarget(BaseEntity boss)
        {
            if (boss == null) return null;

            // Try to get NPC's current target
            var npc = boss as BaseNpc;
            if (npc != null && npc.Target is BasePlayer player)
                return player;

            // Fallback: find closest player within aggro range (20m estimate)
            var closestPlayer = BasePlayer.activePlayerList
                .Where(p => p != null && p.IsConnected && !p.IsSleeping() && Vector3.Distance(p.transform.position, boss.transform.position) <= 20f)
                .OrderBy(p => Vector3.Distance(p.transform.position, boss.transform.position))
                .FirstOrDefault();

            return closestPlayer;
        }

        private SpawnPoint SelectWeightedSpawnpoint(List<SpawnPoint> spawnpoints)
        {
            if (spawnpoints == null || spawnpoints.Count == 0)
                return null;

            if (spawnpoints.Count == 1)
                return spawnpoints[0];

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var sp in spawnpoints)
            {
                totalWeight += Mathf.Max(0.01f, sp.Weight); // Ensure positive weight
            }

            // Roll random value
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;

            // Find selected spawnpoint
            foreach (var sp in spawnpoints)
            {
                accumulated += Mathf.Max(0.01f, sp.Weight);
                if (roll < accumulated)
                    return sp;
            }

            // Fallback (shouldn't reach here)
            return spawnpoints[spawnpoints.Count - 1];
        }

        private void SoftResetBoss(BaseEntity boss, BeastDef def, Vector3 spawnPos)
        {
            if (boss == null || boss.IsDestroyed) return;

            // Teleport back to spawn (no despawn).
            boss.transform.position = spawnPos;
            boss.SendNetworkUpdateImmediate();

            // Restore health.
            var combat = boss as BaseCombatEntity;
            if (combat != null)
            {
                float max = Mathf.Max(1f, def.BaseHealth);
                float healTo = max * Mathf.Clamp01(_config.Leash.ResetHealFraction);
                combat.health = healTo;
                combat.SendNetworkUpdateImmediate();
            }

            // Attempt to calm/stop the AI (best-effort).
            var npc = boss as BaseNpc;
            if (npc != null)
            {
                try { npc.SetFact(BaseNpc.Facts.IsAggro, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsAfraid, 0, true, true); } catch { }
                try { npc.SetFact(BaseNpc.Facts.IsChasing, 0, true, true); } catch { }
            }

            Dbg($"Soft reset boss '{def.DisplayName}' prefab='{boss.ShortPrefabName}' id={boss.net.ID} back to spawn");

            if (_config.Leash.AnnounceResetToChat && _config.Announcements != null && _config.Announcements.Enabled)
            {
                var msg = _config.Leash.ResetMessage.Replace("{name}", def.DisplayName);
                Server.Broadcast(msg);
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

            // Ensure this boss will never flee
            DisableFleeForBoss(entity);

            var driver = entity.gameObject.AddComponent<BeastComponent>();
            driver.Init(this, entity, def);

            _bosses.Add(entity);
            uint bossId = NetId(entity);
            _beastDefs[bossId] = def;
            _bossDamageMultipliers[bossId] = def.DamageMultiplier;
            _bossComponents[bossId] = driver;  // Track component for leash system

            // Apply mythic variant if rolled
            ApplyMythicVariantIfRolled(entity, def, pos);

            Dbg($"Spawned boss '{def.DisplayName}' prefab='{entity.ShortPrefabName}' id={bossId} pos={pos}");

            if (!string.IsNullOrEmpty(def.TierId))
            {
                _activeBossByTier[def.TierId] = entity;
                _bossTierById[bossId] = def.TierId;
                _tierLastDeathTime[def.TierId] = Interface.Oxide.Now;
            }

            // Create world map marker
            CreateBossMarker(entity, def);

            // Announcement on spawn
            if (_config.Announcements.Enabled && _config.Announcements.AnnounceSpawn)
            {
                var spawnMsg = FormatMessage(_config.Announcements.SpawnMessage, def, entity);
                AnnounceToChat(spawnMsg);
            }

            // Proximity warning on spawn
            if (_config.ProximityWarnings.Enabled && _config.ProximityWarnings.WarnOnSpawn)
            {
                var warnMsg = FormatMessage(_config.ProximityWarnings.SpawnMessage, def, entity);
                WarnPlayersNear(entity.transform.position, _config.ProximityWarnings.RadiusMeters, warnMsg);
            }

            // Use mythic display name if applicable
            var displayName = GetBossDisplayName(entity.net.ID.Value, def.DisplayName);
            AnnounceNearby(entity.transform.position, $"<color=#ffdd66>{displayName}</color> has appeared!");

            return entity;
        }

        private void DropConfiguredLoot(uint bossId, Vector3 pos, BeastDef def)
        {
            var container = new ItemContainer();
            int slots = Mathf.Max(6, def.Loot.Count);
            container.ServerInitialize(null, slots);
            container.GiveUID();

            // Check if this boss was a mythic variant for loot multiplier
            bool isMythic = _mythicBossIds.Contains(bossId.Value);
            float lootMultiplier = isMythic ? _config.Mythic.LootMultiplier : 1.0f;

            foreach (var entry in def.Loot)
            {
                var defItem = ItemManager.FindItemDefinition(entry.ShortName);
                if (defItem == null) continue;
                
                // Apply mythic loot multiplier
                int amount = entry.Amount;
                if (isMythic)
                {
                    amount = Mathf.Max(1, Mathf.RoundToInt(entry.Amount * lootMultiplier));
                }
                
                var item = ItemManager.Create(defItem, amount, entry.Skin);
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
            
            if (isMythic)
            {
                Dbg($"Dropped loot for mythic boss with {lootMultiplier}x multiplier");
            }
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
            uint bossId = NetId(entity);
            if (!_bossDamageMultipliers.ContainsKey(bossId))
                _bossDamageMultipliers[bossId] = def.DamageMultiplier;

            _bossDamageMultipliers[bossId] *= def.AbilityEnrage.DamageMultiplier;
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

        private string GetBossDisplayName(uint id, string fallback)
        {
            if (_runtimeBossName.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name))
                return name;
            return fallback;
        }

        private string GetBossTheme(uint id, string fallbackTheme)
        {
            if (_runtimeBossTheme.TryGetValue(id, out var theme) && !string.IsNullOrEmpty(theme))
                return theme;
            return fallbackTheme;
        }

        private string GetBossSubtitle(BeastDef def, uint id)
        {
            // Use runtime theme override if mythic has theme override
            var theme = GetBossTheme(id, def.Theme);
            switch (theme)
            {
                case "frost": return "the Frozen Alpha";
                case "fire": return "the Burning Tyrant";
                case "storm": return "the Storm Herald";
                case "toxic": return "the Plaguebringer";
                case "shadow": return "the Dread Stalker";
                default: return "the Alpha";
            }
        }

        private string BuildTitle(BeastDef def, BaseEntity boss)
        {
            var netId = boss?.net?.ID.Value ?? 0u;
            var name = boss != null ? GetBossDisplayName(netId, def.DisplayName) : def.DisplayName;
            var subtitle = GetBossSubtitle(def, netId);
            return (_config.TitlePlate.TitleFormat ?? "{name}, {subtitle}")
                .Replace("{name}", name)
                .Replace("{subtitle}", subtitle);
        }

        private void DestroyTitlePlate(BasePlayer player)
        {
            if (player == null) return;
            CuiHelper.DestroyUi(player, TitlePlateUi);
        }

        private void DrawTitlePlate(BasePlayer player, uint bossId, BeastDef def, string titleText, string enrageTextOrNull = null, bool pulse = false)
        {
            if (player == null || !player.IsConnected) return;

            var container = new CuiElementContainer();

            // Determine if this is a mythic boss
            bool mythic = IsMythicBoss(bossId);

            // Main panel background with optional pulsing alpha
            var panelColor = _config.TitlePlate.PanelColor;
            if (pulse && _config.EnrageIndicator.Enabled && _config.EnrageIndicator.PulseTitlePlate)
                panelColor = WithAlpha(panelColor, GetPulseAlpha(mythic));

            container.Add(new CuiPanel
            {
                Image = { Color = panelColor },
                RectTransform = { AnchorMin = _config.TitlePlate.PanelAnchorMin, AnchorMax = _config.TitlePlate.PanelAnchorMax },
                CursorEnabled = false
            }, "Overlay", TitlePlateUi);

            // Tier-colored border panels (if BorderThickness > 0)
            if (_config.TitlePlate.BorderThickness > 0f)
            {
                var borderHex = GetBorderHex(def, bossId);
                var bc = ParseColor(borderHex);

                // Apply pulse to border if enraged
                if (pulse && _config.EnrageIndicator.Enabled && _config.EnrageIndicator.PulseBorder)
                    bc.a = GetPulseAlpha(mythic);
                else
                    bc.a = 1f;

                var borderColor = ToCuiColor(bc);
                var t = Mathf.Clamp(_config.TitlePlate.BorderThickness, 0.001f, 0.05f).ToString("F4");

                // Top border
                container.Add(new CuiPanel
                {
                    Image = { Color = borderColor },
                    RectTransform = { AnchorMin = "0 " + (1f - float.Parse(t)).ToString("F4"), AnchorMax = "1 1" },
                    CursorEnabled = false
                }, TitlePlateUi);

                // Bottom border
                container.Add(new CuiPanel
                {
                    Image = { Color = borderColor },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 " + t },
                    CursorEnabled = false
                }, TitlePlateUi);

                // Left border
                container.Add(new CuiPanel
                {
                    Image = { Color = borderColor },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = t + " 1" },
                    CursorEnabled = false
                }, TitlePlateUi);

                // Right border
                container.Add(new CuiPanel
                {
                    Image = { Color = borderColor },
                    RectTransform = { AnchorMin = (1f - float.Parse(t)).ToString("F4") + " 0", AnchorMax = "1 1" },
                    CursorEnabled = false
                }, TitlePlateUi);
            }

            // Optional outline using two labels offset (simple + compatible)
            container.Add(new CuiLabel
            {
                Text = { Text = titleText, FontSize = _config.TitlePlate.FontSize, Align = TextAnchor.MiddleCenter, Color = _config.TitlePlate.OutlineColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1", OffsetMin = "1 -1", OffsetMax = "1 -1" }
            }, TitlePlateUi);

            container.Add(new CuiLabel
            {
                Text = { Text = titleText, FontSize = _config.TitlePlate.FontSize, Align = TextAnchor.MiddleCenter, Color = _config.TitlePlate.TextColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, TitlePlateUi);

            // Optional enrage countdown indicator
            if (!string.IsNullOrEmpty(enrageTextOrNull) && _config.EnrageIndicator.Enabled)
            {
                container.Add(new CuiLabel
                {
                    Text = { Text = enrageTextOrNull, FontSize = _config.EnrageIndicator.FontSize, Align = TextAnchor.MiddleCenter, Color = _config.EnrageIndicator.TextColor },
                    RectTransform = { AnchorMin = _config.EnrageIndicator.AnchorMin, AnchorMax = _config.EnrageIndicator.AnchorMax }
                }, TitlePlateUi);
            }

            CuiHelper.AddUi(player, container);
        }

        private void ApplyMythicVariantIfRolled(BaseEntity boss, BeastDef def, Vector3 pos)
        {
            if (boss == null || boss.net == null) return;
            if (!_config.Mythic.Enabled) return;

            if (UnityEngine.Random.value > Mathf.Clamp01(_config.Mythic.Chance)) return;

            var bossId = boss.net.ID.Value;
            _mythicBossIds.Add(bossId);

            var name = $"{_config.Mythic.NamePrefix}{def.DisplayName}{_config.Mythic.NameSuffix}";
            _runtimeBossName[bossId] = name;

            var theme = string.IsNullOrEmpty(_config.Mythic.ThemeOverride) ? def.Theme : _config.Mythic.ThemeOverride;
            _runtimeBossTheme[bossId] = theme;

            // Spawn FX (mythic)
            var fxKey = string.IsNullOrEmpty(_config.Mythic.SpawnFxKey) ? "enrage_burst" : _config.Mythic.SpawnFxKey;
            var fx = GetRandomFx(fxKey, null, theme);
            if (!string.IsNullOrEmpty(fx))
                Effect.server.Run(fx, pos + Vector3.up * 0.5f);

            Dbg($"Mythic variant spawned: {name} (id={bossId}, theme={theme})");
        }

        private void Dbg(string msg)
        {
            if (_config == null || !_config.Debug) return;
            Puts($"[BeastBoss][DBG] {msg}");
        }

        private void ScaleBeastEntity(BaseEntity entity, float scale)
        {
            // Entity scaling is currently disabled; this is left as a no-op
            // so the method can be reused in the future if scaling is reintroduced.
        }

        private void DisableFleeForBoss(BaseEntity entity)
        {
            if (entity == null)
                return;

            var npc = entity as BaseNpc;
            if (npc == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var type = npc.GetType();

            // Try common flee-related fields; if they exist, force them to "no flee".
            var fleeHealthField = type.GetField("fleeHealthFraction", flags);
            if (fleeHealthField != null)
            {
                try
                {
                    fleeHealthField.SetValue(npc, 0f);
                }
                catch { }
            }

            var canFleeField = type.GetField("canFlee", flags);
            if (canFleeField != null)
            {
                try
                {
                    canFleeField.SetValue(npc, false);
                }
                catch { }
            }

            var usePanicToFleeField = type.GetField("usePanicToFlee", flags);
            if (usePanicToFleeField != null)
            {
                try
                {
                    usePanicToFleeField.SetValue(npc, false);
                }
                catch { }
            }

            // If there is a "fleeIfHurt" or similar boolean, turn it off too.
            var fleeIfHurtField = type.GetField("fleeIfHurt", flags);
            if (fleeIfHurtField != null)
            {
                try
                {
                    fleeIfHurtField.SetValue(npc, false);
                }
                catch { }
            }

            // We don't strictly need a network update for AI flags,
            // but it is safe to send one in case any networked state changed.
            npc.SendNetworkUpdate();

            Dbg($"DisableFleeForBoss applied to {npc.ShortPrefabName} ({npc.net?.ID})");
        }

        #endregion

        #region WorldEventPipeline

        private void StartWorldEvents()
        {
            if (!_config.WorldEvents.Enabled) return;
            Dbg("World events enabled; scheduling first event...");
            ScheduleNextWorldEvent();
        }

        private void ScheduleNextWorldEvent()
        {
            if (_worldEventTimer != null)
            {
                _worldEventTimer.Destroy();
                _worldEventTimer = null;
            }

            float nextMinutes = UnityEngine.Random.Range(
                Mathf.Max(1f, _config.WorldEvents.MinMinutesBetweenEvents),
                Mathf.Max(1f, _config.WorldEvents.MaxMinutesBetweenEvents)
            );

            _worldEventTimer = timer.Once(nextMinutes * 60f, () =>
            {
                TryRunWorldEvent();
                ScheduleNextWorldEvent();
            });

            Dbg($"Next world event scheduled in {nextMinutes:F1} minutes");
        }

        private void TryRunWorldEvent()
        {
            // Select a target player for this world event (per-player tier selection)
            var candidates = BasePlayer.activePlayerList.Where(p => p != null && p.IsConnected).ToList();
            if (candidates.Count == 0)
            {
                Dbg("TryRunWorldEvent: no online players");
                return;
            }

            var targetPlayer = candidates.GetRandom();
            string targetTier = GetPlayerCurrentTier(targetPlayer.userID);

            BeastDef def = SelectEventBeastDefForTier(targetTier, _config.WorldEvents.AllowedBeastKeys);
            if (def == null)
            {
                Dbg($"TryRunWorldEvent: no eligible beast def found for tier '{targetTier}'");
                return;
            }

            Vector3? spawnPos = SelectWeightedSpawnPoint(_config.WorldEvents.SpawnpointGroup);
            if (spawnPos == null)
            {
                Dbg($"TryRunWorldEvent: no spawn position found for group '{_config.WorldEvents.SpawnpointGroup}'");
                return;
            }

            Dbg($"World event spawning '{def.DisplayName}' at {spawnPos} for player {targetPlayer.displayName} (tier {targetTier})");
            SpawnBeast(def, spawnPos.Value);
        }

        private BeastDef SelectEventBeastDefForTier(string tierId, List<string> allowedKeys)
        {
            // Build eligible pool
            var eligible = new List<string>();

            // Start with all beasts
            foreach (var key in _config.Beasts.Keys)
                eligible.Add(key);

            // Filter by AllowedBeastKeys if set
            if (allowedKeys != null && allowedKeys.Count > 0)
            {
                eligible = eligible.Where(k => allowedKeys.Contains(k)).ToList();
            }

            if (eligible.Count == 0)
            {
                Dbg("SelectEventBeastDefForTier: no eligible beasts after allowed filter");
                return null;
            }

            // Filter by tier
            if (_config.TierEscalation.Enabled && !string.IsNullOrEmpty(tierId))
            {
                var tieredEligible = eligible.Where(k =>
                {
                    var def = _config.Beasts[k];
                    return string.Equals(def.TierId, tierId, StringComparison.OrdinalIgnoreCase);
                }).ToList();

                // If tier filter yields results, use it; otherwise fall back to all
                if (tieredEligible.Count > 0)
                {
                    eligible = tieredEligible;
                    Dbg($"SelectEventBeastDefForTier: filtered to tier '{tierId}', {eligible.Count} candidates");
                }
            }

            if (eligible.Count == 0)
            {
                Dbg("SelectEventBeastDefForTier: no eligible beasts after tier filter");
                return null;
            }

            // Choose randomly
            string chosenKey = eligible.GetRandom();
            return _config.Beasts[chosenKey];
        }

        private Vector3? SelectWeightedSpawnPoint(string group)
        {
            if (!_data.Spawnpoints.TryGetValue(group, out var spawnList) || spawnList.Count == 0)
                return null;

            if (spawnList.Count == 1)
                return spawnList[0].Position.ToVector3();

            // Weighted selection
            float totalWeight = 0f;
            foreach (var sp in spawnList)
            {
                if (sp.Weight > 0)
                    totalWeight += sp.Weight;
            }

            if (totalWeight <= 0)
                return spawnList[0].Position.ToVector3(); // fallback

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;
            foreach (var sp in spawnList)
            {
                if (sp.Weight > 0)
                {
                    accumulated += sp.Weight;
                    if (roll <= accumulated)
                        return sp.Position.ToVector3();
                }
            }

            return spawnList[spawnList.Count - 1].Position.ToVector3();
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
            if (_beastDefs.TryGetValue(NetId(boss), out var def))
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
            var userId = player.userID;
            CuiHelper.DestroyUi(player, "BeastBossHUD");
            RemoveHud(userId);
            RestoreExternalHud(userId);
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

            // Enrage timing (for countdown display)
            private float _enrageEndsAt;

            // Weather proc tracking
            private float _nextWeatherProcCheck;

            // Leash system tracking
            private Vector3 _spawnPos;
            private float _outsideLeashSince = -1f;

            // Return-to-spawn state
            private bool _isReturning;
            private float _returnStartedAt;
            private float _nextReturnDestAt;

            public Vector3 SpawnPos => _spawnPos;
            public BaseEntity Entity => _entity;
            public BeastDef Def => _def;
            public bool IsReturning => _isReturning;
            public bool IsEnraged => _enraged;
            public float EnrageSecondsRemaining
            {
                get
                {
                    if (!_enraged) return 0f;
                    var rem = _enrageEndsAt - Time.realtimeSinceStartup;
                    return rem > 0f ? rem : 0f;
                }
            }

            public void Init(BeastBoss plugin, BaseEntity entity, BeastDef def)
            {
                _plugin = plugin;
                _entity = entity;
                _combat = entity as BaseCombatEntity;
                _animal = entity as BaseAnimalNPC;
                _def = def;

                // Store spawn position for leash system
                _spawnPos = entity.transform.position;
                _outsideLeashSince = -1f;

                // Initialize returning state
                _isReturning = false;
                _returnStartedAt = 0f;
                _nextReturnDestAt = 0f;

                // Initialize enrage timing
                _enraged = false;
                _enrageEndsAt = 0f;

                // Ensure flee behavior is disabled even if something resets stats later.
                _plugin.DisableFleeForBoss(entity);

                var now = Time.realtimeSinceStartup;
                _nextRoar = now + UnityEngine.Random.Range(4f, 8f);
                _nextCharge = now + UnityEngine.Random.Range(8f, 12f);
                _nextFrost = now + UnityEngine.Random.Range(10f, 14f);
                _nextFireTrail = now + UnityEngine.Random.Range(10f, 16f);
                _nextWeatherProcCheck = now + UnityEngine.Random.Range(1f, 2f);

                InvokeRepeating(nameof(Tick), 0.5f, 0.25f);
            }

            private void OnDestroy()
            {
                CancelInvoke(nameof(Tick));
            }

            public void SetReturning(bool returning)
            {
                _isReturning = returning;
                if (returning)
                {
                    _returnStartedAt = Time.realtimeSinceStartup;
                    _nextReturnDestAt = 0f;
                }
            }

            private void Tick()
            {
                if (_entity == null || _entity.IsDestroyed)
                {
                    CancelInvoke(nameof(Tick));
                    return;
                }

                // Check if enrage duration has expired
                if (_enraged && Time.realtimeSinceStartup >= _enrageEndsAt)
                {
                    _enraged = false;
                    _enrageEndsAt = 0f;
                }

                // Return-to-spawn state machine (if enabled)
                if (_plugin._config.Leash.Enabled && _plugin._config.Leash.WalkBackToSpawn)
                {
                    // If we are currently returning, manage the return-to-spawn journey
                    if (_isReturning)
                    {
                        var now = Time.realtimeSinceStartup;
                        var npc = _entity as BaseNpc;

                        // Refresh destination periodically
                        if (npc != null && now >= _nextReturnDestAt)
                        {
                            _plugin.TrySetNpcDestination(npc, _spawnPos);
                            _nextReturnDestAt = now + Mathf.Max(0.5f, _plugin._config.Leash.ReturnDestinationRefreshSeconds);
                        }

                        // If close enough to spawn, finish reset
                        if (Vector3.Distance(_entity.transform.position, _spawnPos) <= Mathf.Max(1f, _plugin._config.Leash.ReturnStopDistanceMeters))
                        {
                            _plugin.CompleteReturnReset(_entity, _def);
                            SetReturning(false);
                            _outsideLeashSince = -1f;
                            return;
                        }

                        // Stuck fail-safe
                        if (_plugin._config.Leash.ReturnMaxSeconds > 0f && (now - _returnStartedAt) >= _plugin._config.Leash.ReturnMaxSeconds)
                        {
                            if (_plugin._config.Leash.FallbackTeleportIfStuck)
                            {
                                _entity.transform.position = _spawnPos;
                                _entity.SendNetworkUpdateImmediate();
                                _plugin.CompleteReturnReset(_entity, _def);
                            }
                            else
                            {
                                _plugin.CompleteReturnReset(_entity, _def);
                            }

                            SetReturning(false);
                            _outsideLeashSince = -1f;
                            return;
                        }

                        // While returning, skip combat ability logic (boss is walking home)
                        return;
                    }

                    // If not returning, evaluate leash breach and decide to start returning
                    if (_plugin.IsBossBeyondHardReset(_entity, _spawnPos))
                    {
                        _plugin.BeginReturnToSpawn(this);
                        return;
                    }

                    if (_plugin.IsBossOutsideLeash(_entity, _spawnPos))
                    {
                        var now = Time.realtimeSinceStartup;
                        if (_outsideLeashSince < 0f) _outsideLeashSince = now;
                        else if (_plugin._config.Leash.ResetAfterSecondsOutside > 0f &&
                                 (now - _outsideLeashSince) >= _plugin._config.Leash.ResetAfterSecondsOutside)
                        {
                            _plugin.BeginReturnToSpawn(this);
                            return;
                        }
                    }
                    else
                    {
                        _outsideLeashSince = -1f;
                    }
                }

                var now2 = Time.realtimeSinceStartup;

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

                if (_def.AbilityRoar.Enabled && now2 >= _nextRoar)
                {
                    DoRoar();
                    _nextRoar = now2 + Mathf.Max(5f, _def.AbilityRoar.Interval);
                }

                if (_def.AbilityCharge.Enabled && now2 >= _nextCharge)
                {
                    DoCharge();
                    _nextCharge = now2 + Mathf.Max(8f, _def.AbilityCharge.Interval);
                }

                if (_def.AbilityFrostAura.Enabled && now2 >= _nextFrost)
                {
                    DoFrostAura();
                    _nextFrost = now2 + Mathf.Max(8f, _def.AbilityFrostAura.Interval);
                }

                if (_def.AbilityFireTrail.Enabled && now2 >= _nextFireTrail)
                {
                    DoFireTrail();
                    _nextFireTrail = now2 + Mathf.Max(8f, _def.AbilityFireTrail.Interval);
                }

                // Weather-enhanced proc checks (lightning strikes, etc.)
                CheckWeatherProc(now2);
            }

            private void CheckWeatherProc(float now)
            {
                if (!_plugin._config.WeatherProcs.Enabled) return;

                // Get runtime theme (includes mythic overrides)
                var theme = _plugin.GetBossTheme(_entity.net.ID.Value, _def.Theme);

                // Check theme filter
                if (_plugin._config.WeatherProcs.StormThemeOnly && theme != "storm")
                    return;

                // Check bad weather requirement
                if (_plugin._config.WeatherProcs.RequireBadWeather && !_plugin.IsBadWeather())
                    return;

                // Check if enraged (only proc while enraged)
                if (!_enraged)
                    return;

                // Check proc interval
                if (now >= _nextWeatherProcCheck)
                {
                    _nextWeatherProcCheck = now + Mathf.Max(1f, _plugin._config.WeatherProcs.ProcCheckIntervalSeconds);

                    // Roll for proc chance
                    if (UnityEngine.Random.value <= _plugin._config.WeatherProcs.ProcChancePerCheck)
                    {
                        var target = _plugin.GetPrimaryTarget(_entity);
                        if (target != null)
                        {
                            _plugin.DoLightningProc(_entity, target, _plugin._config.WeatherProcs.ProcDamage, _plugin._config.WeatherProcs.ProcRadiusMeters, theme);
                        }
                    }
                }
            }

            #region Abilities

            private void CheckPhaseScaling()
            {
                // Phase-based scaling is disabled; this method is kept as a stub
                // so it can be re-enabled in the future if needed.
                return;
            }

            private void StartEnrageAura(float duration)
            {
                if (duration <= 0f)
                    return;

                // How often to pulse the aura FX while enraged.
                const float Interval = 1.0f;
                int ticks = Mathf.Max(1, Mathf.RoundToInt(duration / Interval));

                _plugin.timer.Repeat(Interval, ticks, () =>
                {
                    if (_entity == null || _entity.IsDestroyed)
                        return;

                    var pos = _entity.transform.position + Vector3.up * 0.25f;

                    // Use themed enrage aura if available, otherwise generic.
                    var auraFx = _plugin.GetRandomFx("enrage_aura", null, _def.Theme);
                    if (!string.IsNullOrEmpty(auraFx))
                    {
                        Effect.server.Run(auraFx, pos);
                    }
                });
            }

            private void DoRoar()
            {
                var s = _def.AbilityRoar;
                var origin = _entity.transform.position + Vector3.up * 0.8f;

                // Pick a random roar FX (or fallback)
                var roarFx = _plugin.GetRandomFx("roar_blast", null, _def.Theme);
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
                    var impactFx = _plugin.GetRandomFx("ground_impact", null, _def.Theme);
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
                var castFx = _plugin.GetRandomFx("frost_aura", null, _def.Theme);
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

                    // Ensure summoned helpers never flee
                    _plugin.DisableFleeForBoss(child);

                    _plugin.Dbg($"Summoned helper prefab='{child.ShortPrefabName}' id={child.net.ID} pos={spawnPos} for boss='{_def.DisplayName}' bossId={_entity.net.ID}");

                    var childCombat = child as BaseCombatEntity;
                    if (childCombat != null)
                    {
                        childCombat.InitializeHealth(_def.BaseHealth * 0.25f, _def.BaseHealth * 0.25f);
                    }

                    Effect.server.Run(
                        _plugin.GetRandomFx("summon_burst", null, _def.Theme),
                        spawnPos
                    );
                }

                _plugin.PrintBossChat($"<color=#ffb700>{_def.DisplayName}</color> summons allies to its side!");
            }

            private void TriggerEnrage()
            {
                var s = _def.AbilityEnrage;

                _plugin.ApplyEnrageBuff(_entity, _def);

                _plugin.Dbg($"Enrage triggered for '{_def.DisplayName}' prefab='{_entity.ShortPrefabName}' id={_entity.net.ID} hp={_combat?.health}/{_def.BaseHealth}");

                // Announcement on enrage
                if (_plugin._config.Announcements.Enabled && _plugin._config.Announcements.AnnounceEnrage)
                {
                    var enrageMsg = _plugin.FormatMessage(_plugin._config.Announcements.EnrageMessage, _def, _entity);
                    _plugin.AnnounceToChat(enrageMsg);
                }

                // Proximity warning on enrage
                if (_plugin._config.ProximityWarnings.Enabled && _plugin._config.ProximityWarnings.WarnOnEnrage)
                {
                    var warnMsg = _plugin.FormatMessage(_plugin._config.ProximityWarnings.EnrageMessage, _def, _entity);
                    _plugin.WarnPlayersNear(_entity.transform.position, _plugin._config.ProximityWarnings.RadiusMeters, warnMsg);
                }

                // Start visual enrage aura for the duration of the enrage effect.
                StartEnrageAura(s.Duration);

                var pos = _entity.transform.position + Vector3.up * 0.5f;

                var fx = _plugin.GetRandomFx("enrage_burst", s.EffectPrefab, _def.Theme);
                if (!string.IsNullOrEmpty(fx))
                    Effect.server.Run(fx, pos);

                // Mythic enrage FX overlay with runtime theme
                if (_entity?.net != null && _plugin._mythicBossIds.Contains(_entity.net.ID.Value))
                {
                    var theme = _plugin.GetBossTheme(_entity.net.ID.Value, _def.Theme);
                    var key = string.IsNullOrEmpty(_plugin._config.Mythic.EnrageFxKey) ? "enrage_aura" : _plugin._config.Mythic.EnrageFxKey;
                    var mythicFx = _plugin.GetRandomFx(key, null, theme);
                    if (!string.IsNullOrEmpty(mythicFx))
                    {
                        Effect.server.Run(mythicFx, pos);
                        _plugin.Dbg($"Mythic enrage FX triggered for boss id={_entity.net.ID}");
                    }
                }

                // Track enrage timing for countdown display
                _enraged = true;
                float dur = 0f;
                try { dur = _def.AbilityEnrage != null ? _def.AbilityEnrage.Duration : 0f; } catch { dur = 0f; }
                _enrageEndsAt = Time.realtimeSinceStartup + Mathf.Max(0f, dur);

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

                    var trailFx = _plugin.GetRandomFx("fire_trail", null, _def.Theme);
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
