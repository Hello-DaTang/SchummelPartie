// The tests execute the production lifecycle, settings and present patches without
// loading Unity's native player. Only engine/UI and unrelated minigames are replaced.
using System.Reflection;
using SchummelPartie.module;

namespace UnityEngine
{
    public class Object
    {
        public bool Destroyed;
        public static bool operator ==(Object left, Object right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null) || right.Destroyed;
            if (ReferenceEquals(right, null)) return left.Destroyed;
            return ReferenceEquals(left, right);
        }
        public static bool operator !=(Object left, Object right) => !(left == right);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
    public enum KeyCode { F8 }
    public static class Input
    {
        public static readonly HashSet<KeyCode> Pressed = new();
        public static bool GetKeyDown(KeyCode key) => Pressed.Contains(key);
    }
    public enum FontStyle { Bold }
    public struct Color { public static Color white; }
    public class GUIStyleState { public Color textColor; }
    public class GUIStyle
    {
        public int fontSize;
        public GUIStyleState normal = new();
        public FontStyle fontStyle;
    }
}

namespace MelonLoader
{
    public static class MelonLogger
    {
        public static int Warnings, Errors;
        public static void Msg(string text) { }
        public static void Warning(string text) => Warnings++;
        public static void Error(string text) => Errors++;
    }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type, string method) { }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HarmonyPrefix : Attribute { }
    public static class AccessTools
    {
        public static FieldInfo Field(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }
    }
}

namespace UrGUI.UWindow
{
    public static class UWindowControls
    {
        public sealed class WToggle { public bool Value; }
    }
    public class UWindow
    {
        public bool IsDrawing;
        public static UWindow Begin(string name, int x, int y, int width, int startHeight, bool dynamicHeight) => new();
        public void Separator() { }
        public void SameLine() { }
        public void Label(string text) { }
        public UWindowControls.WToggle Toggle(string name, Action<bool> change, bool value) => new() { Value = value };
    }
}

namespace SchummelPartie.setting
{
    public static class SettingManager
    {
        public static List<ISetting> Settings { get; } = new();
        public static List<ISetting> Get(string container) => Settings.Where(setting => setting.Container == container).ToList();
    }
}

namespace ZP.Net
{
    public class NetBehaviour : UnityEngine.Object { public bool IsOwner; }
}

public class CharacterBase : UnityEngine.Object
{
    public bool IsOwner;
    public GamePlayer GamePlayer;
}
public sealed class BoardPlayer : CharacterBase { }
public sealed class GamePlayer
{
    public bool IsAI;
    public BoardPlayer BoardObject;
}
public class Item : ZP.Net.NetBehaviour { protected GamePlayer player; public void SetPlayer(GamePlayer value) => player = value; }
public sealed class PresentItem : Item { }
public static class GameManager
{
    public static bool DEBUGGING;
    public static object Minigame;
}

namespace SchummelPartie.module
{
    public static class LocalFreeCamera
    {
        public static int StopCount;
        public static bool Active;
        public static void Stop() { Active = false; StopCount++; }
        public static void OnUpdate() { }
    }
}

namespace SchummelPartie.module.modules
{
    public class DummyModule : Module { public DummyModule() : base("Dummy", "") { } }
    public sealed class ModuleGUI : Module
    {
        public static ModuleGUI Instance;
        public int Updates;
        public ModuleGUI() : base("GUI", "") { Instance = this; }
        protected override bool AvailableWhileSuspended => true;
        public override void OnUpdate() => Updates++;
    }
    public sealed class ModuleAnimalArithmetic : DummyModule { }
    public sealed class ModuleMysteryMaze : DummyModule { }
    public sealed class ModuleMortarMayhem : DummyModule { }
    public sealed class ModuleBattyBatter : DummyModule { }
    public sealed class ModuleSpookySpikes : DummyModule { }
    public sealed class ModuleRythm : DummyModule { }
    public sealed class ModulePackAndPile : DummyModule { }
    public sealed class ModuleTreasureHunt : DummyModule { }
    public sealed class ModuleSwiftShooters : DummyModule { }
    public sealed class ModuleBomber : DummyModule { }
    public sealed class ModuleFinder : DummyModule { }
    public sealed class ModuleBarnBrawl : DummyModule { }
    public sealed class ModulePresents : DummyModule { }
    public sealed class ModuleSelfishStride : DummyModule { }
    public sealed class ModuleMemoryMenu : DummyModule { }
    public sealed class ModuleTanks : DummyModule { }
    public sealed class ModuleSidestepSlope : DummyModule { }
    public sealed class ModuleDaringDogfight : DummyModule { }
    public sealed class ModuleElementalMages : DummyModule { }
    public sealed class ModuleCrownCapture : DummyModule { }
    public sealed class ModuleExplosiveExchange : DummyModule { }
    public sealed class ModuleAirJump : DummyModule { }
    public sealed class ModuleSpeed : DummyModule { }
    public sealed class ModuleESP : DummyModule { }
}
