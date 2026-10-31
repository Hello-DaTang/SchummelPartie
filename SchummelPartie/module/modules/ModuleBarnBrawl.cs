using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using SchummelPartie.render;
using SchummelPartie.setting.settings;
using UnityEngine;

namespace SchummelPartie.module.modules;

public class ModuleBarnBrawl : ModuleMinigame<BarnBrawlController>
{
    private static readonly FieldInfo CameraShakeField = typeof(BarnBrawlPlayer)
        .GetField("cameraShake", BindingFlags.NonPublic | BindingFlags.Instance);
    public SettingSwitch BurstShotgun;
    public SettingSwitch GodMode;
    public SettingSwitch InfiniteShotgun;
    public SettingSwitch NoCameraShake;

    public ModuleBarnBrawl() : base("Barn Brawl", "God Mode, Infinite Shotgun (Press F), Burst Shotgun, ESP.")
    {
        GodMode = new SettingSwitch(Name, "God Mode");
        InfiniteShotgun = new SettingSwitch(Name, "Infinite Shotgun");
        BurstShotgun = new SettingSwitch(Name, "Burst Shotgun");
        NoCameraShake = new SettingSwitch(Name, "No Camera Shake");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Enabled)
            if (GameManager.Minigame is BarnBrawlController barnBrawlController)
                foreach (var player in barnBrawlController.players)
                    if (player is BarnBrawlPlayer barnBrawlPlayer)
                        if (player.IsMe())
                        {
                            if ((bool)GodMode.GetValue())
                                barnBrawlPlayer.ApplyDamage(barnBrawlPlayer, -10, Vector3.zero, 0, 0, 0);

                            if (!barnBrawlPlayer.HoldingShotgun)
                                if ((bool)InfiniteShotgun.GetValue() || Input.GetKeyDown(KeyCode.F))
                                    Overrides.Set(barnBrawlPlayer, "HoldingShotgun", () => barnBrawlPlayer.HoldingShotgun,
                                        value => barnBrawlPlayer.HoldingShotgun = value, true);

                            if ((bool)BurstShotgun.GetValue())
                            {
                                Overrides.Set(barnBrawlPlayer, "minProjectiles", () => barnBrawlPlayer.minProjectiles,
                                    value => barnBrawlPlayer.minProjectiles = value, 50);
                                Overrides.Set(barnBrawlPlayer, "maxProjectiles", () => barnBrawlPlayer.maxProjectiles,
                                    value => barnBrawlPlayer.maxProjectiles = value, 60);
                            }
                            else
                            {
                                Overrides.Restore(barnBrawlPlayer, "minProjectiles");
                                Overrides.Restore(barnBrawlPlayer, "maxProjectiles");
                            }

                            var cameraShake = CameraShakeField?.GetValue(barnBrawlPlayer) as CameraShake;
                            if (cameraShake != null)
                            {
                                if ((bool)NoCameraShake.GetValue())
                                    Overrides.Set(cameraShake, "enabled", () => cameraShake.enabled,
                                        value => cameraShake.enabled = value, false);
                                else
                                    Overrides.Restore(cameraShake, "enabled");
                            }
                        }
    }
}
