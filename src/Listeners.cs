using CounterStrikeSharp.API;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (!Config.Warmup && _warmup)
            return;

        if (Config.NoRushTime != 0 && !_bombPlanted)
        {
            var diff = (Config.NoRushTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["rushDisabled"], diff.ToString("0")]}");
            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");
        }

        if (Config.NoCampTime != 0)
        {
            var diff = (Config.NoCampTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["campEnabled"], diff.ToString("0")]}");
            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["campEnabled"]}");
        }

        foreach (var controller in Utilities.GetPlayers().Where(c => c.IsValid() && c.PawnIsAlive))
        {
            foreach (var zone in _zones)
            {
                if (((Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime) || _bombPlanted) && Config.RushZones.Contains((int)zone.Type))
                    continue;

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && Config.CampZones.Contains((int)zone.Type))
                    continue;

                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!isInZone)
                {
                    if (zone.Data[controller].Entry != 0)
                    {
                        zone.Data[controller].Entry = 0;
                        zone.Data[controller].Exit = Server.CurrentTime;
                    }
                    
                    continue;
                }

                if (zone.Data[controller].Entry == 0)
                {
                    zone.Data[controller].Entry = Server.CurrentTime;
                    zone.Data[controller].Exit = 0;
                }

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (zone.Delay != 0)
                {
                    var diff = (zone.Data[controller].Entry + zone.Delay) - Server.CurrentTime;
                    float progressPercentage = diff / zone.Delay;
                    string color = GetColorBasedOnProgress(progressPercentage);
                    string progressBar = GenerateProgressBar(progressPercentage);

                    if (diff > 0)
                    {
                        var diffString = diff % 1;

                        if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                            {
                                controller.PrintToCenterHtml(
                                    $"<font class='fontSize-m' color='yellow'>MOVE!</font><br>" +
                                    $"<font class='fontSize-s' color='white'>NO CAMPING HERE [{diff.ToString("0")}]</font><br>" +
                                    $"<font class='fontSize-l' color='{color}'>{progressBar}</font>"
                                );
                            }
                    }
                    else
                        DoAction(controller, zone);

                    continue;
                }

                DoAction(controller, zone);
            }
        }

        return;
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var controller = Utilities.GetPlayerFromSlot(playerSlot);

        if (controller == null || !controller.IsValid())
            return;

        _playerData[controller] = new PlayerData();
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