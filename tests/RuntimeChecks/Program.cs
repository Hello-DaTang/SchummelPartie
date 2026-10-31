using SchummelPartie.module;
using SchummelPartie.module.modules;
using SchummelPartie.setting;
using UnityEngine;
using Module = SchummelPartie.module.Module;

var checks = new (string Name, Action Run)[]
{
    ("Pause preserves settings, restores values and disables Debug", PauseAndResume),
    ("Module toggles invoke lifecycle hooks once", ToggleLifecycle),
    ("Settings edited while paused take effect only on resume", EditWhilePaused),
    ("F8 works with the GUI hidden; automation stays stopped", HotkeyWithHiddenGui),
    ("Repeated writes retain the original value; a new activation resnapshots", RepeatedWrites),
    ("Partial restore and reflected fields retain their own originals", ReflectedFields),
    ("Destroyed objects and a failed restore do not block other restores", RestoreFailures),
    ("Scene changes restore parameters and stop the free camera", SceneChanges),
    ("Present patches force the same good result locally and in RPCs", OwnPresent),
    ("Other players, AI, unrelated RPCs and malformed arguments stay untouched", OtherPresents),
    ("Paused or disabled present patches leave the random result untouched", PausedPresents),
    ("A failing module disable still restores its values and other modules", DisableFailure),
};
foreach (var check in checks)
{
    Reset();
    check.Run();
    Console.WriteLine("PASS: " + check.Name);
}
Console.WriteLine($"{checks.Length} runtime checks passed.");

static void Reset()
{
    ModuleManager.Modules.Clear();
    ModuleManager.SetSuspended(false);
    Input.Pressed.Clear();
    SettingManager.Settings.Clear();
    GameManager.DEBUGGING = false;
    LocalFreeCamera.StopCount = 0;
    LocalFreeCamera.Active = false;
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}, got {actual}.");
}

static void PauseAndResume()
{
    var active = new Probe();
    var disabled = new Probe();
    disabled.EnabledSetting.SetValue(false);
    var gui = new ModuleGUI();
    ModuleManager.Modules.AddRange(new Module[] { active, disabled, gui, new ModuleDebug() });
    ModuleManager.RefreshStates();
    var target = new Target { Speed = 4 };
    active.Overrides.Set(target, "speed", () => target.Speed, value => target.Speed = value, 20);
    LocalFreeCamera.Active = true;
    ModuleManager.SetSuspended(true);
    Equal(4, target.Speed);
    Equal(true, active.ConfiguredEnabled);
    Equal(false, active.Enabled);
    Equal(false, disabled.ConfiguredEnabled);
    Equal(true, gui.Enabled);
    Equal(false, GameManager.DEBUGGING);
    Equal(false, LocalFreeCamera.Active);
    ModuleManager.SetSuspended(true);
    Equal(1, active.Disables);
    ModuleManager.SetSuspended(false);
    Equal(true, active.Enabled);
    Equal(false, disabled.Enabled);
    Equal(true, GameManager.DEBUGGING);
    Equal(2, active.Enables);
}

static void ToggleLifecycle()
{
    var probe = new Probe();
    probe.RefreshState();
    probe.Toggle();
    Equal(1, probe.Disables);
    probe.Toggle();
    Equal(2, probe.Enables);
    probe.EnabledSetting.SetValue(true);
    Equal(2, probe.Enables);
}

static void EditWhilePaused()
{
    var probe = new Probe();
    ModuleManager.Modules.Add(probe);
    ModuleManager.RefreshStates();
    ModuleManager.SetSuspended(true);
    probe.Toggle();
    probe.Toggle();
    Equal(false, probe.Enabled);
    Equal(1, probe.Enables);
    ModuleManager.SetSuspended(false);
    Equal(2, probe.Enables);
    ModuleManager.SetSuspended(true);
    probe.EnabledSetting.SetValue(false);
    ModuleManager.SetSuspended(false);
    Equal(false, probe.Enabled);
}

static void HotkeyWithHiddenGui()
{
    var probe = new Probe();
    var gui = new ModuleGUI();
    gui.EnabledSetting.SetValue(false);
    ModuleManager.Modules.AddRange(new Module[] { probe, gui });
    ModuleManager.RefreshStates();
    Input.Pressed.Add(KeyCode.F8);
    ModuleManager.OnUpdate();
    Input.Pressed.Clear();
    ModuleManager.OnUpdate();
    Equal(true, ModuleManager.IsSuspended);
    Equal(0, probe.Updates);
    Equal(2, gui.Updates);
    Input.Pressed.Add(KeyCode.F8);
    ModuleManager.OnUpdate();
    Input.Pressed.Clear();
    Equal(false, ModuleManager.IsSuspended);
    Equal(1, probe.Updates);
    Equal(false, gui.ConfiguredEnabled);
}

static void RepeatedWrites()
{
    var state = new RuntimeOverrides();
    var target = new Target { Speed = 7 };
    for (var index = 0; index < 100; index++)
        state.Set(target, "speed", () => target.Speed, value => target.Speed = value, 100 + index);
    state.RestoreAll();
    Equal(7, target.Speed);
    target.Speed = 11;
    state.Set(target, "speed", () => target.Speed, value => target.Speed = value, 40);
    state.RestoreAll();
    Equal(11, target.Speed);
}

static void ReflectedFields()
{
    var state = new RuntimeOverrides();
    var target = new Target { Speed = 5, Rate = 3 };
    var field = typeof(Target).GetField(nameof(Target.Rate));
    state.SetField(target, field, 0.001f);
    state.Set(target, "speed", () => target.Speed, value => target.Speed = value, 50);
    state.Restore(target, nameof(Target.Rate));
    Equal(3f, target.Rate);
    Equal(50, target.Speed);
    state.RestoreAll();
    Equal(5, target.Speed);
}

static void RestoreFailures()
{
    var state = new RuntimeOverrides();
    var broken = new Target();
    var destroyed = new Target();
    var good = new Target { Speed = 2 };
    state.Remember(broken, "broken", () => throw new Exception("test failure"));
    state.Remember(destroyed, "gone", () => throw new Exception("must never execute"));
    destroyed.Destroyed = true;
    state.Set(good, "speed", () => good.Speed, value => good.Speed = value, 50);
    var warnings = MelonLoader.MelonLogger.Warnings;
    state.RestoreAll();
    Equal(warnings + 1, MelonLoader.MelonLogger.Warnings);
    Equal(2, good.Speed);
    state.RestoreAll();
    Equal(warnings + 1, MelonLoader.MelonLogger.Warnings);
}

static void SceneChanges()
{
    var probe = new Probe();
    var target = new Target { Speed = 8 };
    ModuleManager.Modules.Add(probe);
    probe.Overrides.Set(target, "speed", () => target.Speed, value => target.Speed = value, 80);
    LocalFreeCamera.Active = true;
    ModuleManager.OnSceneChanged();
    Equal(8, target.Speed);
    Equal(false, LocalFreeCamera.Active);
    Equal(true, probe.ConfiguredEnabled);
}

static PresentItem Present(bool owner = true, bool ai = false)
{
    var present = new PresentItem { IsOwner = owner };
    present.SetPlayer(new GamePlayer { IsAI = ai, BoardObject = new BoardPlayer { IsOwner = owner } });
    return present;
}

static void OwnPresent()
{
    ModuleManager.Modules.Add(new ModuleForcePresent());
    var present = Present();
    byte action = 2;
    PresentItemPatch.Prefix(present, ref action);
    Equal((byte)1, action);
    object[] parameters = { (byte)2, 71, new ulong[3] };
    NetBehaviourPatch.Prefix(present, "RPCOpenPresent", parameters);
    Equal((byte)1, (byte)parameters[0]);
    Equal(71, (int)parameters[1]);
}

static void OtherPresents()
{
    ModuleManager.Modules.Add(new ModuleForcePresent());
    var mismatchedOwner = Present();
    mismatchedOwner.IsOwner = false;
    foreach (var present in new[] { Present(false), Present(true, true), mismatchedOwner, new PresentItem(), null })
    {
        byte action = 2;
        PresentItemPatch.Prefix(present, ref action);
        Equal((byte)2, action);
        object[] parameters = { (byte)2, 7, new ulong[3] };
        NetBehaviourPatch.Prefix(present, "RPCOpenPresent", parameters);
        Equal((byte)2, (byte)parameters[0]);
    }
    object[] unrelated = { (byte)2, 7, new ulong[3] };
    NetBehaviourPatch.Prefix(Present(), "OtherRPC", unrelated);
    NetBehaviourPatch.Prefix(new ZP.Net.NetBehaviour { IsOwner = true }, "RPCOpenPresent", unrelated);
    NetBehaviourPatch.Prefix(Present(), "RPCOpenPresent", null);
    NetBehaviourPatch.Prefix(Present(), "RPCOpenPresent", Array.Empty<object>());
    object[] incomplete = { (byte)2 };
    NetBehaviourPatch.Prefix(Present(), "RPCOpenPresent", incomplete);
    Equal((byte)2, (byte)incomplete[0]);
    Equal((byte)2, (byte)unrelated[0]);
}

static void PausedPresents()
{
    var module = new ModuleForcePresent();
    ModuleManager.Modules.Add(module);
    ModuleManager.RefreshStates();
    ModuleManager.SetSuspended(true);
    byte action = 2;
    PresentItemPatch.Prefix(Present(), ref action);
    object[] parameters = { (byte)2, 7, new ulong[3] };
    NetBehaviourPatch.Prefix(Present(), "RPCOpenPresent", parameters);
    Equal((byte)2, action);
    Equal((byte)2, (byte)parameters[0]);
    ModuleManager.SetSuspended(false);
    module.EnabledSetting.SetValue(false);
    PresentItemPatch.Prefix(Present(), ref action);
    Equal((byte)2, action);
}

static void DisableFailure()
{
    var broken = new Probe { ThrowOnDisable = true };
    var good = new Probe();
    var target = new Target { Speed = 9 };
    ModuleManager.Modules.AddRange(new Module[] { broken, good });
    ModuleManager.RefreshStates();
    broken.Overrides.Set(target, "speed", () => target.Speed, value => target.Speed = value, 90);
    ModuleManager.SetSuspended(true);
    Equal(9, target.Speed);
    Equal(1, good.Disables);
    Equal(false, broken.Enabled);
}

sealed class Target : UnityEngine.Object { public int Speed; public float Rate; }
sealed class Probe : Module
{
    public int Enables, Disables, Updates;
    public bool ThrowOnDisable;
    public Probe() : base("Probe", "") { }
    protected override void OnEnable() => Enables++;
    protected override void OnDisable()
    {
        Disables++;
        if (ThrowOnDisable) throw new Exception("test failure");
    }
    public override void OnUpdate() => Updates++;
}
