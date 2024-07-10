using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid || !controller.PlayerPawn.IsValid || controller.PlayerPawn.Value == null)
            return HookResult.Continue;

        if (_playerData.TryGetValue(controller, out var value))
            value.SpawnPos = new Vector(controller.PlayerPawn.Value!.AbsOrigin!.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z);

        return HookResult.Continue;
    }

    private HookResult OnBulletImpact(EventBulletImpact @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (!IsValidPlayer(controller) || !_playerData.TryGetValue(controller!, out var value) || value.AddZone == null || !Menu.IsCurrentMenu(controller!, value.AddZone))
            return HookResult.Continue;

        if (!value.AddZone.Points[0].IsZero() && !value.AddZone.Points[1].IsZero())
            return HookResult.Continue;

        if (Server.CurrentTime - value.AddZone.LastShot < 0.1)
            return HookResult.Continue;

        value.AddZone.LastShot = Server.CurrentTime;

        if (value.AddZone.Points[0].IsZero())
            value.AddZone.Points[0] = new Vector(@event.X, @event.Y, @event.Z);
        else
        {
            value.AddZone.Points[1] = new Vector(@event.X, @event.Y, @event.Z);

            var diff = Math.Abs(value.AddZone.Points[0].Z - value.AddZone.Points[1].Z);

            if (diff < 200)
            {
                if (value.AddZone.Points[0].Z >= value.AddZone.Points[1].Z)
                    value.AddZone.Points[0].Z += 200 - diff;
                else
                    value.AddZone.Points[1].Z += 200 - diff;
            }
        }

        Menu.PopMenu(controller!, value.AddZone);
        BuildAddZoneMenu(controller!);

        return HookResult.Continue;
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid)
            return HookResult.Continue;

        _playerData[controller] = new PlayerData();

        return HookResult.Continue;
    }

    public static bool isBombPlanted = false;
    [GameEventHandler]
    public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        isBombPlanted = true;
        return HookResult.Continue;

    }

    [GameEventHandler]
    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        isBombPlanted = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombDetonate(EventBombExploded @event, GameEventInfo info)
    {
        isBombPlanted = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        isBombPlanted = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        isBombPlanted = false;
        return HookResult.Continue;
    }

    public static bool[] justSpawned = new bool[64];

    [GameEventHandler]
    public HookResult OnPlayerSpawned(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player is null || player.IsBot || player.IsHLTV || !player.IsValid)
            return HookResult.Continue;

        justSpawned[player.Index] = true;
        AddTimer(10.0f, () => justSpawned[player.Index] = false);
        return HookResult.Continue; 
    }
}