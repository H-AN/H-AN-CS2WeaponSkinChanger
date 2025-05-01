using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Drawing;
using System.Reflection;
using CounterStrikeSharp.API.Core.Attributes;

public static class SoundSystem {

    public static MemoryFunctionVoid<CBaseEntity, string, int, float, float> CBaseEntity_EmitSoundParamsFunc = new(GameData.GetSignature("CBaseEntity_EmitSoundParams")); 

    public static void EmitSound(this CBaseEntity entity, string soundEventName, int pitch = 1, float volume = 1f, float delay = 1f)
    {
        if (entity is null 
        || entity.IsValid is not true
        || string.IsNullOrEmpty(soundEventName) is true 
        || CBaseEntity_EmitSoundParamsFunc is null) return;

        //invoke play sound from an entity
        CBaseEntity_EmitSoundParamsFunc.Invoke(entity, soundEventName, pitch, volume, delay);
    }

    public static void EmitSoundGlobal(string soundEventName)
    {
        //get the world entity so we can emit global sounds
        var worldEntity = Utilities.GetEntityFromIndex<CBaseEntity>(0);

        if (worldEntity is null 
        || worldEntity.IsValid is not true
        || string.IsNullOrEmpty(soundEventName) is true  
        || worldEntity.DesignerName.Contains("world") is not true) return;

        //emit sound from the worldent
        worldEntity.EmitSound(soundEventName);
        
    }

    
    //鸣谢laoshi的EmitSoundParams代码
    public static void EmitSoundParams(this CCSPlayerController entity, string soundPath, int pitch = 100, float volume = 1.0f, float delay = 0.0f)
    {
        if(entity == null || string.IsNullOrEmpty(soundPath))
        return;

        MemoryFunctionVoid<CBaseEntity, string, int, float, float> CBaseEntity_EmitSoundParamsFunc = new MemoryFunctionVoid<CBaseEntity, string, int, float, float>
        (GameData.GetSignature("CBaseEntity_EmitSoundParams"));
        CBaseEntity_EmitSoundParamsFunc.Invoke(entity, soundPath, pitch, volume, delay);
    }

/*
    public static void StopSound(this CBaseEntity entity, string soundEventName)
    {
        if (entity is null 
        || entity.IsValid is not  true
        || string.IsNullOrEmpty(soundEventName) is true 
        || CBaseEntity_StopSoundEvent is null) return;

        //invoke stop sound comming from an entity
        CBaseEntity_StopSoundEvent.Invoke(entity.Handle, soundEventName);

    }
*/

}