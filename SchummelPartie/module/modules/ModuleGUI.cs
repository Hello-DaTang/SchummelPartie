using UnityEngine;
using UrGUI.UWindow;

namespace SchummelPartie.module.modules;

public class ModuleGUI : Module
{
    private readonly UWindow _window;
    private readonly UWindowControls.WToggle _pauseToggle;
    private readonly UWindowControls.WToggle _cameraToggle;

    protected override bool AvailableWhileSuspended => true;

    private readonly string[] _achievements = new[]
    {
        "ACH_ALTITUDE_ATTACK",
        "ACH_WIN_FIRST_GAME",
        "ACH_FIRST_GAME",
        "ACH_BARN_BRAWL",
        "ACH_FIRST_GOAL",
        "ACH_MADE_A_MISTAKE",
        "ACH_BREAKING_BLOCKS",
        "ACH_BULLET_BARRAGE",
        "ACH_GRIFTING_GIFTS",
        "ACH_THE_TRIFECTA",
        "ACH_ELEMENTAL_ESCALATION",
        "ACH_UNLUCKY",
        "ACH_MINIGAME_MASTER",
        "ACH_SLIPPERY_SPRINT",
        "ACH_THUNDEROUS_TRENCH",
        "ACH_EXPLOSIVE_EXCHANGE",
        "ACH_BOUNCING_BALLS",
        "ACH_GIFT_GRAB",
        "ACH_RUSTY_RACERS",
        "ACH_WIN_FIRST_MINIGAME",
        "ACH_EXTRA_MEAT",
        "ACH_SORCERERS_SPRINT",
        "ACH_TEMPORAL_TRAILS",
        "ACH_SNOWY_SPIN",
        "ACH_SPEEDY_SABERS",
        "ACH_DAMAGE_SPOTLIGHTS",
        "ACH_TUNNELING_TANKS",
        "ACH_SANDY_SEARCH",
        "ACH_ACIDIC_ATOLL",
        "ACH_MAGMA_MAGES",
        "ACH_WINTER_MAZE"
    };

    public ModuleGUI() : base("Graphical User Interface", "Toggle the GUI with Insert or RightShift.")
    {
        Instance = this;
        _window = UWindow.Begin("Schummel Partie by Lyzev", 0, 0, 350, startHeight: 400, dynamicHeight: true);
        _pauseToggle = _window.Toggle("Pause all cheats (F8)", ModuleManager.SetSuspended, false);
        _cameraToggle = _window.Toggle("Local free camera (F11)", active =>
        {
            if (active) LocalFreeCamera.Start();
            else LocalFreeCamera.Stop();
        }, false);
        _window.Button("Pause / Resume all (F8)", ModuleManager.ToggleSuspended);
        _window.Separator();
        _window.SameLine();
        _window.Button("Discord", () => Application.OpenURL("https://lyzev.github.io/discord/"));
        _window.Button("GitHub", () => Application.OpenURL("https://github.com/Lyzev/SchummelPartie"));
        _window.Button("Unlock all achievements", () =>
        {
            if (ModuleManager.IsSuspended) return;
            foreach (var achievement in _achievements)
                if (!PlatformAchievementManager.Instance.HasUnlockedAchievement(achievement))
                    PlatformAchievementManager.Instance.TriggerAchievement(achievement, null);
        });
        _window.IsDrawing = false;
    }

    public static ModuleGUI Instance { get; private set; }

    public void InitSettings()
    {
        ModuleManager.OnSettings(_window);
    }

    protected override void OnEnable()
    {
        _window.IsDrawing = true;
    }

    protected override void OnDisable()
    {
        _window.IsDrawing = false;
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Insert) || Input.GetKeyDown(KeyCode.RightShift)) Toggle();
        _pauseToggle.Value = ModuleManager.IsSuspended;
        _cameraToggle.Value = LocalFreeCamera.Active;
    }

    public override void OnGUI()
    {
        _pauseToggle.Value = ModuleManager.IsSuspended;
        _cameraToggle.Value = LocalFreeCamera.Active;
        if (ModuleManager.IsSuspended)
            GUI.Box(new Rect(Screen.width / 2f - 160, 8, 320, 30), "CHEATS PAUSED - F8 to resume");
        else if (LocalFreeCamera.Active)
            GUI.Box(new Rect(Screen.width / 2f - 230, 8, 460, 30), "FREE CAMERA: WASD / Q,E / RMB / Shift | F11 exits");
    }
}
