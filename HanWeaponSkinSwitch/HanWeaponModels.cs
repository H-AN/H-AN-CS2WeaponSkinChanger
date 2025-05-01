using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.Json;
using System.Drawing;
using CounterStrikeSharp.API.Core.Capabilities;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
public static class Weapon
{
    public static string GetDesignerName(CBasePlayerWeapon weapon)
    {
        string weaponDesignerName = weapon.DesignerName;
        ushort weaponIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;

        weaponDesignerName = (weaponDesignerName, weaponIndex) switch
        {
            var (name, _) when name.Contains("bayonet") => "weapon_knife",
            ("weapon_m4a1", 60) => "weapon_m4a1_silencer",
            ("weapon_hkp2000", 61) => "weapon_usp_silencer",
            _ => weaponDesignerName
        };

        return weaponDesignerName;
    }

    public static string GetViewModel(CCSPlayerController player)
    {
        var viewModel = ViewModel(player)?.VMName ?? string.Empty;
        return viewModel;
    }

    public static void SetViewModel(CCSPlayerController player, string model)
    {
        ViewModel(player)?.SetModel(model);
    }

    public static void UpdateModel(CCSPlayerController player, CBasePlayerWeapon weapon, string model, bool update)
    {
        weapon.Globalname = $"{GetViewModel(player)},{model}";
        weapon.SetModel(model);

        if (update)
            SetViewModel(player, model);
    }

    public static void ResetWeapon(CCSPlayerController player, CBasePlayerWeapon weapon, bool update)
    {
        string globalname = weapon.Globalname;

        if (string.IsNullOrEmpty(globalname))
            return;

        string[] globalnamedata = globalname.Split(',');

        weapon.Globalname = string.Empty;
        weapon.SetModel(globalnamedata[0]);

        if (update)
            SetViewModel(player, globalnamedata[0]);
    }

    public static bool HandleEquip(CCSPlayerController player, string modelName, bool isEquip)
    {
        if (player.PawnIsAlive)
        {
            var weaponpart = modelName.Split(':');
            if (weaponpart.Length != 2)
                return false;

            var weaponName = weaponpart[0];
            var weaponModel = weaponpart[1];

            CBasePlayerWeapon? weapon = Get(player, weaponName);

            if (weapon != null)
            {
                bool equip = weapon == player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;

                if (isEquip)
                    UpdateModel(player, weapon, weaponModel, equip);

                else ResetWeapon(player, weapon, equip);

                return true;
            }

            else return false;
        }

        return true;
    }
    private static CBasePlayerWeapon? Get(CCSPlayerController player, string weaponName)
    {
        CPlayer_WeaponServices? weaponServices = player.PlayerPawn?.Value?.WeaponServices;

        if (weaponServices == null)
            return null;

        CBasePlayerWeapon? activeWeapon = weaponServices.ActiveWeapon?.Value;

        if (activeWeapon != null && GetDesignerName(activeWeapon) == weaponName)
            return activeWeapon;

        return weaponServices.MyWeapons.SingleOrDefault(p => p.Value != null && GetDesignerName(p.Value) == weaponName)?.Value;
    }
    private static CBaseViewModel? ViewModel(CCSPlayerController player)
    {
        nint? handle = player.PlayerPawn.Value?.ViewModelServices?.Handle;

        if (handle == null || !handle.HasValue)
            return null;

        CCSPlayer_ViewModelServices viewModelServices = new(handle.Value);

        nint ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        Span<nint> viewModels = MemoryMarshal.CreateSpan(ref ptr, 3);

        CHandle<CBaseViewModel> viewModel = new(viewModels[0]);

        return viewModel.Value;
    }

    public static bool EquipWeapon(CCSPlayerController player, string model)
    {
        return Weapon.HandleEquip(player, model, true);
    } 
    
}