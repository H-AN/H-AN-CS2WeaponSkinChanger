using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Commands;




namespace HanWeapon;

[MinimumApiVersion(80)]

public class HanWeaponSkinPlugin : BasePlugin
{

    public override string ModuleName => "[华仔]武器皮肤";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "By : 华仔H-AN";
    public override string ModuleDescription => "武器皮肤";

    private Dictionary<string, WeaponData> _weaponData = new();

    private static HanWeaponSkinPlugin _instance;

	public HanWeaponSkinPlugin()
	{
		_instance = this; // 在构造函数中存储实例
	}

    public override void Load(bool hotReload)
    { 

        LoadConfigs();
        RegisterListeners();
        RegisterEventHandler<EventItemEquip>(OnItemEquip);

    }
    private void LoadConfigs()
    {
        try 
        {
            // 加载主武器配置
            var mainPath = Path.Combine(ModuleDirectory, "../../configs/plugins/Shop/HanPrimaryWeapon.json");
            var mainJson = JObject.Parse(File.ReadAllText(mainPath));
            
            // 加载副武器具配置
            var specialPath = Path.Combine(ModuleDirectory, "../../configs/plugins/Shop/HanSecondWeapon.json");
            var specialJson = File.Exists(specialPath) ? JObject.Parse(File.ReadAllText(specialPath)) : new JObject();

            // 加载空投武器具配置
            var AirPath = Path.Combine(ModuleDirectory, "../../configs/plugins/Shop/HanAirWeapon.json");
            var AirJson = File.Exists(AirPath) ? JObject.Parse(File.ReadAllText(AirPath)) : new JObject();

            _weaponData = mainJson.Properties()
            .Concat(specialJson.Properties())
            .Concat(AirJson.Properties())
            .GroupBy(p => p.Name)  // 按名称分组
            .ToDictionary(
                g => g.Key,  // 分组键（武器名称）
                g => new WeaponData {
                    Name = g.First().Value["name"]?.ToString(),
                    ModelPath = g.First().Value["model"]?.ToString() ?? g.First().Value["Vmodels"]?.ToString(),
                    WModelPath = g.First().Value["Wmodels"]?.ToString()
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载配置失败: {ex.Message}");
        }
    }

    private void RegisterListeners()
    {
        RegisterListener<Listeners.OnServerPrecacheResources>(manifest => 
        {
            foreach (var weapon in _weaponData.Values)
            {
                if (!string.IsNullOrWhiteSpace(weapon.ModelPath))
                {
                    manifest.AddResource(weapon.ModelPath);
                }
                // 预缓存 W 模型（可选）
                if (!string.IsNullOrWhiteSpace(weapon.WModelPath))
                {
                    manifest.AddResource(weapon.WModelPath);
                }
            }
            
        });
    }

    private static HookResult OnItemEquip(EventItemEquip @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || 
            player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is not { } weapon ||
            player.IsBot || player.TeamNum != 3)
        {
            return HookResult.Continue;
        }
        // 通过武器自定义名称查找对应模型
        var customName = weapon.AttributeManager.Item.CustomName;
        if (!string.IsNullOrEmpty(customName) && 
            _instance._weaponData.TryGetValue(_instance.GetKeyFromName(customName), out var iweapon))
        {
            string SwitchName = Weapon.GetDesignerName(weapon);

            Weapon.EquipWeapon(player, $"{SwitchName}:{iweapon.ModelPath}");
            // 如果存在 W 模型，则设置
            if (!string.IsNullOrWhiteSpace(iweapon.WModelPath))
            {
                weapon.SetModel(iweapon.WModelPath);
            }
        }

        return HookResult.Continue; 
    }
    private string GetKeyFromName(string displayName)
    {
        return _weaponData.FirstOrDefault(x => x.Value.Name == displayName).Key ?? displayName.ToLower();
    }

}

public class WeaponData
{
    public string Name { get; set; }
    public string ModelPath { get; set; }
    public string WModelPath { get; set; } // 新增 W 模型路径
}

    

