﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

[MinimumApiVersion(245)]
public partial class AntiRush
{
    public override string ModuleName => "AntiRush";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "https://github.com/oscar-wos/AntiRush";
    public Menu.Menu Menu { get; } = new();

    private string Prefix { get; } = $"{ChatColors.Red}ANTICAMP {ChatColors.White}| ";
    private List<Zone> _zones = [];
    private readonly Dictionary<CCSPlayerController, PlayerData> _playerData = [];
}