using System.Reflection;
using HarmonyLib;
using MelonLoader;

namespace SchummelPartie.module.modules;

public class ModuleMysteryMaze : ModuleMinigame<MysteryMazeController>
{
    public ModuleMysteryMaze() : base("Mystery Maze", "Shows the path to the exit.")
    {
        Instance = this;
    }

    public static ModuleMysteryMaze Instance { get; private set; }
}

[HarmonyPatch(typeof(MysteryMazeController), "Update")]
public static class MysteryMazeControllerPatch
{
    private static readonly FieldInfo RadiusField = AccessTools.Field(typeof(MysteryMazeController), "m_clipRadius");

    [HarmonyPostfix]
    internal static void Postfix(MysteryMazeController __instance)
    {
        if (ModuleMysteryMaze.Instance.Enabled && RadiusField != null)
        {
            // Override the rendering only; leave the game's visibility events and target radius intact.
            ModuleMysteryMaze.Instance.Overrides.Remember(__instance, "_ClipRadiusOverride", () =>
                UnityEngine.Shader.SetGlobalFloat("_ClipRadiusOverride", (float)RadiusField.GetValue(__instance)));
            UnityEngine.Shader.SetGlobalFloat("_ClipRadiusOverride", 50f);
        }
    }
}
