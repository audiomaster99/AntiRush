using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        foreach (var controller in Utilities.GetPlayers().Where(player => player is { IsValid: true, PawnIsAlive: true } && _playerData.ContainsKey(player)))
        {
            var bounce = false;

            foreach (var zone in _zones)
            {
                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!isInZone || justSpawned[controller.Index])
                {
                    zone.Entry.Remove(controller);
                    continue;
                }

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (!zone.Entry.ContainsKey(controller))
                    zone.Entry[controller] = Server.CurrentTime;

                

                if (zone.Delay != 0)
                {
                    var diff = (zone.Entry[controller] + zone.Delay) - Server.CurrentTime;

                    float progressPercentage = diff / zone.Delay;
                    string color = GetColorBasedOnProgress(progressPercentage);
                    string progressBar = GenerateProgressBar(progressPercentage);

                    if (diff > 0)
                    {
                        var diffString = diff % 1;

                        if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1.0)
                        {
                            //controller.PrintToChat($"{Prefix}{Localizer["delayRemaining", FormatZoneString(zone.Type), diff.ToString("0")]}");
                            controller.PrintToCenterHtml(
                                $"<font class='fontSize-m' color='yellow'>WARNING</font><br>" +
                                $"<font class='fontSize-s' color='white'>YOU ENTERED NOCAMP ZONE [{diff.ToString("0")}]</font><br>" +
                                $"<font class='fontSize-l' color='{color}'>{progressBar}</font>"
                            );
                        }
                    }
                    else
                        bounce = DoAction(controller, zone);

                    continue;
                }

                bounce = DoAction(controller, zone);
            }
            
            if (bounce)
                continue;

            _playerData[controller].LastPosition = new Vector(controller.PlayerPawn.Value!.AbsOrigin!.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z);
            _playerData[controller].LastVelocity = new Vector(controller.PlayerPawn.Value.AbsVelocity.X, controller.PlayerPawn.Value.AbsVelocity.Y, controller.PlayerPawn.Value.AbsVelocity.Z);
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private string GenerateProgressBar(float progress)
    {
        int totalBars = 19;
        int filledBars = (int)(totalBars * progress);

        string filledPart = new string('█', filledBars);
        string emptyPart = new string('░', totalBars - filledBars);
        return $"{filledPart}{emptyPart}";
    }

    private string GetColorBasedOnProgress(float progress)
    {
        // Ensure progress is within the range [0, 1]
        progress = Math.Clamp(progress, 0, 1);

        int red, green, blue = 0;

        if (progress < 0.5f)
        {
            // From red to yellow (progress 0 to 0.5)
            red = 255;
            green = (int)(255 * (progress * 2));
        }
        else
        {
            // From yellow to green (progress 0.5 to 1)
            red = (int)(255 * (2 * (1 - progress)));
            green = 255;
        }

        return $"#{red:X2}{green:X2}{blue:X2}";
    }
}
