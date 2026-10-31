using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using SchummelPartie.setting.settings;
using ZP.Net;

namespace SchummelPartie.module.modules;

public class ModuleForcePresent : Module
{
    public const byte GoodActionId = 1;
    private static readonly FieldInfo PlayerField = AccessTools.Field(typeof(Item), "player");

    public ModuleForcePresent() : base("Force Present", "Your existing presents always give the good teleport result. No items are granted.")
    {
        Instance = this;
    }

    public static ModuleForcePresent Instance { get; private set; }

    internal static bool ShouldForce(PresentItem present)
    {
        if (Instance == null || !Instance.Enabled || present == null || !present.IsOwner || PlayerField == null) return false;
        var player = PlayerField.GetValue(present) as GamePlayer;
        return player != null && !player.IsAI && player.BoardObject != null && player.BoardObject.IsOwner;
    }
}

[HarmonyPatch(typeof(PresentItem), "OpenPresent")]
public static class PresentItemPatch
{
    [HarmonyPrefix]
    internal static void Prefix(PresentItem __instance, ref byte actionID)
    {
        if (ModuleForcePresent.ShouldForce(__instance)) actionID = ModuleForcePresent.GoodActionId;
    }
}

[HarmonyPatch(typeof(NetBehaviour), "SendRPC")]
public static class NetBehaviourPatch
{
    [HarmonyPrefix]
    internal static void Prefix(NetBehaviour __instance, string method,
        params object[] parameters)
    {
        if (method == "RPCOpenPresent" && __instance is PresentItem present &&
            parameters != null && parameters.Length == 3 && parameters[0] is byte &&
            ModuleForcePresent.ShouldForce(present))
            parameters[0] = ModuleForcePresent.GoodActionId;
    }
}
