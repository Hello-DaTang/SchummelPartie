using System;
using System.Reflection;
using MelonLoader;
using SchummelPartie.setting.settings;
using UnityEngine;

namespace SchummelPartie.module.modules;

public class ModuleElementalMages : ModuleMinigame<ElementalMagesController>
{
    public SettingSwitch InstantPickupCrystal;
    public SettingSwitch NoCameraShake;

    public ModuleElementalMages() : base("Elemental Mages", "Instantly pick up crystals and disable camera shake.")
    {
        InstantPickupCrystal = new SettingSwitch(Name, "Instant Pickup Crystal (Host Only)");
        NoCameraShake = new SettingSwitch(Name, "No Camera Shake");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Enabled)
            if (GameManager.Minigame is ElementalMagesController elementalMagesController)
                foreach (var player in elementalMagesController.players)
                    if (player is ElementalMagesPlayer elementalMagesPlayer)
                        if (player.IsMe())
                        {
                            if ((bool)InstantPickupCrystal.GetValue())
                                try
                                {
                                    foreach (var crystal in elementalMagesController.crystals)
                                        elementalMagesController.RPCDespawnCrystal(null, crystal.id, (byte)player.OwnerSlot);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                            // This minigame has a shared camera; its player has no cameraShake field.
                            var cameraShake = elementalMagesController.MinigameCamera != null
                                ? elementalMagesController.MinigameCamera.GetComponentInParent<CameraShake>()
                                : null;
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
