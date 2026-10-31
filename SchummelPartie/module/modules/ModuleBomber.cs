namespace SchummelPartie.module.modules;

public class ModuleBomber : ModuleMinigame<BomberController>
{
    public ModuleBomber() : base("Bomber", "Bombs are infinite.")
    {
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Enabled)
            if (GameManager.Minigame is BomberController bomberController)
                foreach (var player in bomberController.players)
                    if (player is BomberPlayer bomberPlayer)
                        if (player.IsMe())
                        {
                            Overrides.Set(bomberPlayer, "bombRange", () => bomberPlayer.bombRange,
                                value => bomberPlayer.bombRange = value, 1000);
                            Overrides.Set(bomberPlayer, "MaxBombs", () => bomberPlayer.MaxBombs,
                                value => bomberPlayer.MaxBombs = value, 1000);
                            var previousBombs = bomberPlayer.BombsRemaining;
                            Overrides.Remember(bomberPlayer, "BombsRemaining", () =>
                                bomberPlayer.BombsRemaining = System.Math.Min(previousBombs, bomberPlayer.BombsRemaining));
                            bomberPlayer.BombsRemaining = 1000;
                        }
    }
}
