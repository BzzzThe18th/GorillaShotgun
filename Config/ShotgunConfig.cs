using System;
using System.IO;
using BepInEx.Configuration;
using BepInEx;

namespace GorillaShotgun.Config
{
    static internal class ShotgunConfig
    {
        public static ConfigEntry<float> volume;
        public static ConfigEntry<float> force;
        public static ConfigEntry<bool> isLeft;
        public static ConfigEntry<bool> enabled;

        public static void LoadConfig()
        {
            var file = new ConfigFile(Path.Combine(Paths.ConfigPath, "GorillaShotgunConfig.cfg"), true);

            volume = file.Bind("All", "SFX Volume", 5f, "changes volume of shoot and load SFX");
            force = file.Bind("All", "Knockback Force", 1f, "changes force at which the shotgun knocks you back");
            isLeft = file.Bind("All", "Left Handed", false, "changes which hand the shotgun is in");
            enabled = file.Bind("All", "Enabled", true, "enables or disables the mod");
        }
    }
}
