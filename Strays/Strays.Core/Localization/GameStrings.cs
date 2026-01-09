namespace Strays.Core.Localization;

/// <summary>
/// Static class containing all localization string keys for the game.
/// Using constants ensures compile-time safety and IDE autocompletion.
/// </summary>
public static class GameStrings
{
    #region Common UI

    public const string OK = "UI_OK";
    public const string Cancel = "UI_Cancel";
    public const string Confirm = "UI_Confirm";
    public const string Back = "UI_Back";
    public const string Close = "UI_Close";
    public const string Yes = "UI_Yes";
    public const string No = "UI_No";
    public const string Accept = "UI_Accept";
    public const string Decline = "UI_Decline";
    public const string Continue = "UI_Continue";
    public const string Skip = "UI_Skip";
    public const string Save = "UI_Save";
    public const string Load = "UI_Load";
    public const string Delete = "UI_Delete";
    public const string Edit = "UI_Edit";
    public const string Apply = "UI_Apply";
    public const string Reset = "UI_Reset";
    public const string Default = "UI_Default";

    #endregion

    #region Main Menu

    public const string Menu_NewGame = "Menu_NewGame";
    public const string Menu_Continue = "Menu_Continue";
    public const string Menu_LoadGame = "Menu_LoadGame";
    public const string Menu_Settings = "Menu_Settings";
    public const string Menu_Credits = "Menu_Credits";
    public const string Menu_Quit = "Menu_Quit";
    public const string Menu_QuitConfirm = "Menu_QuitConfirm";

    #endregion

    #region Pause Menu

    public const string Pause_Title = "Pause_Title";
    public const string Pause_Resume = "Pause_Resume";
    public const string Pause_Inventory = "Pause_Inventory";
    public const string Pause_Party = "Pause_Party";
    public const string Pause_Quests = "Pause_Quests";
    public const string Pause_Map = "Pause_Map";
    public const string Pause_Settings = "Pause_Settings";
    public const string Pause_MainMenu = "Pause_MainMenu";
    public const string Pause_MainMenuConfirm = "Pause_MainMenuConfirm";

    #endregion

    #region Settings

    public const string Settings_Title = "Settings_Title";
    public const string Settings_Audio = "Settings_Audio";
    public const string Settings_Video = "Settings_Video";
    public const string Settings_Controls = "Settings_Controls";
    public const string Settings_Gameplay = "Settings_Gameplay";
    public const string Settings_Accessibility = "Settings_Accessibility";
    public const string Settings_Language = "Settings_Language";

    // Audio
    public const string Settings_MasterVolume = "Settings_MasterVolume";
    public const string Settings_MusicVolume = "Settings_MusicVolume";
    public const string Settings_SFXVolume = "Settings_SFXVolume";
    public const string Settings_VoiceVolume = "Settings_VoiceVolume";
    public const string Settings_AmbientVolume = "Settings_AmbientVolume";

    // Video
    public const string Settings_Resolution = "Settings_Resolution";
    public const string Settings_Fullscreen = "Settings_Fullscreen";
    public const string Settings_VSync = "Settings_VSync";
    public const string Settings_Brightness = "Settings_Brightness";

    // Controls
    public const string Settings_Rebind = "Settings_Rebind";
    public const string Settings_Sensitivity = "Settings_Sensitivity";
    public const string Settings_InvertY = "Settings_InvertY";
    public const string Settings_Vibration = "Settings_Vibration";

    // Accessibility
    public const string Settings_Subtitles = "Settings_Subtitles";
    public const string Settings_SubtitleSize = "Settings_SubtitleSize";
    public const string Settings_ScreenShake = "Settings_ScreenShake";
    public const string Settings_ColorblindMode = "Settings_ColorblindMode";

    #endregion

    #region Combat

    public const string Combat_Attack = "Combat_Attack";
    public const string Combat_Defend = "Combat_Defend";
    public const string Combat_Ability = "Combat_Ability";
    public const string Combat_Item = "Combat_Item";
    public const string Combat_Flee = "Combat_Flee";
    public const string Combat_Switch = "Combat_Switch";

    public const string Combat_Victory = "Combat_Victory";
    public const string Combat_Defeat = "Combat_Defeat";
    public const string Combat_Escaped = "Combat_Escaped";

    public const string Combat_Damage = "Combat_Damage";
    public const string Combat_Critical = "Combat_Critical";
    public const string Combat_Miss = "Combat_Miss";
    public const string Combat_Blocked = "Combat_Blocked";
    public const string Combat_Healed = "Combat_Healed";
    public const string Combat_Resisted = "Combat_Resisted";
    public const string Combat_Weak = "Combat_Weak";

    public const string Combat_YourTurn = "Combat_YourTurn";
    public const string Combat_EnemyTurn = "Combat_EnemyTurn";
    public const string Combat_SelectTarget = "Combat_SelectTarget";
    public const string Combat_SelectAbility = "Combat_SelectAbility";
    public const string Combat_NotEnoughEnergy = "Combat_NotEnoughEnergy";

    public const string Combat_XPGained = "Combat_XPGained";
    public const string Combat_CurrencyGained = "Combat_CurrencyGained";
    public const string Combat_ItemDropped = "Combat_ItemDropped";
    public const string Combat_LevelUp = "Combat_LevelUp";

    #endregion

    #region Strays

    public const string Stray_Level = "Stray_Level";
    public const string Stray_HP = "Stray_HP";
    public const string Stray_Energy = "Stray_Energy";
    public const string Stray_Attack = "Stray_Attack";
    public const string Stray_Defense = "Stray_Defense";
    public const string Stray_Speed = "Stray_Speed";
    public const string Stray_Type = "Stray_Type";

    public const string Stray_Recruited = "Stray_Recruited";
    public const string Stray_Released = "Stray_Released";
    public const string Stray_Evolved = "Stray_Evolved";
    public const string Stray_EvolutionAvailable = "Stray_EvolutionAvailable";

    public const string Stray_Abilities = "Stray_Abilities";
    public const string Stray_Stats = "Stray_Stats";
    public const string Stray_Info = "Stray_Info";

    public const string Stray_Recruit = "Stray_Recruit";
    public const string Stray_Release = "Stray_Release";
    public const string Stray_Evolve = "Stray_Evolve";
    public const string Stray_ViewDetails = "Stray_ViewDetails";

    public const string Stray_Wild = "Stray_Wild";
    public const string Stray_Companion = "Stray_Companion";
    public const string Stray_Boss = "Stray_Boss";

    #endregion

    #region Items

    public const string Item_Use = "Item_Use";
    public const string Item_Equip = "Item_Equip";
    public const string Item_Unequip = "Item_Unequip";
    public const string Item_Drop = "Item_Drop";
    public const string Item_Sell = "Item_Sell";
    public const string Item_Buy = "Item_Buy";

    public const string Item_Consumable = "Item_Consumable";
    public const string Item_Equipment = "Item_Equipment";
    public const string Item_KeyItem = "Item_KeyItem";
    public const string Item_Material = "Item_Material";

    public const string Item_Rarity_Common = "Item_Rarity_Common";
    public const string Item_Rarity_Uncommon = "Item_Rarity_Uncommon";
    public const string Item_Rarity_Rare = "Item_Rarity_Rare";
    public const string Item_Rarity_Epic = "Item_Rarity_Epic";
    public const string Item_Rarity_Legendary = "Item_Rarity_Legendary";

    public const string Item_Quantity = "Item_Quantity";
    public const string Item_Value = "Item_Value";
    public const string Item_Obtained = "Item_Obtained";
    public const string Item_Used = "Item_Used";

    #endregion

    #region Inventory

    public const string Inventory_Title = "Inventory_Title";
    public const string Inventory_Empty = "Inventory_Empty";
    public const string Inventory_Full = "Inventory_Full";
    public const string Inventory_Currency = "Inventory_Currency";
    public const string Inventory_Weight = "Inventory_Weight";
    public const string Inventory_Sort = "Inventory_Sort";
    public const string Inventory_Filter = "Inventory_Filter";

    #endregion

    #region Shop

    public const string Shop_Title = "Shop_Title";
    public const string Shop_Buy = "Shop_Buy";
    public const string Shop_Sell = "Shop_Sell";
    public const string Shop_Confirm = "Shop_Confirm";
    public const string Shop_NoFunds = "Shop_NoFunds";
    public const string Shop_SoldOut = "Shop_SoldOut";
    public const string Shop_Thanks = "Shop_Thanks";

    #endregion

    #region Quests

    public const string Quest_Title = "Quest_Title";
    public const string Quest_Active = "Quest_Active";
    public const string Quest_Completed = "Quest_Completed";
    public const string Quest_Failed = "Quest_Failed";

    public const string Quest_Main = "Quest_Main";
    public const string Quest_Side = "Quest_Side";
    public const string Quest_Bounty = "Quest_Bounty";

    public const string Quest_Objectives = "Quest_Objectives";
    public const string Quest_Rewards = "Quest_Rewards";
    public const string Quest_Progress = "Quest_Progress";

    public const string Quest_New = "Quest_New";
    public const string Quest_Updated = "Quest_Updated";
    public const string Quest_Complete = "Quest_Complete";

    public const string Quest_Track = "Quest_Track";
    public const string Quest_Abandon = "Quest_Abandon";
    public const string Quest_AbandonConfirm = "Quest_AbandonConfirm";

    #endregion

    #region Map / Navigation

    public const string Map_Title = "Map_Title";
    public const string Map_CurrentLocation = "Map_CurrentLocation";
    public const string Map_FastTravel = "Map_FastTravel";
    public const string Map_FastTravelConfirm = "Map_FastTravelConfirm";
    public const string Map_Unexplored = "Map_Unexplored";
    public const string Map_Marker = "Map_Marker";

    #endregion

    #region Biomes

    public const string Biome_Fringe = "Biome_Fringe";
    public const string Biome_Rust = "Biome_Rust";
    public const string Biome_Green = "Biome_Green";
    public const string Biome_Quiet = "Biome_Quiet";
    public const string Biome_Teeth = "Biome_Teeth";
    public const string Biome_Glow = "Biome_Glow";
    public const string Biome_Archive = "Biome_Archive";

    public const string Biome_FringeDesc = "Biome_FringeDesc";
    public const string Biome_RustDesc = "Biome_RustDesc";
    public const string Biome_GreenDesc = "Biome_GreenDesc";
    public const string Biome_QuietDesc = "Biome_QuietDesc";
    public const string Biome_TeethDesc = "Biome_TeethDesc";
    public const string Biome_GlowDesc = "Biome_GlowDesc";
    public const string Biome_ArchiveDesc = "Biome_ArchiveDesc";

    #endregion

    #region Factions

    public const string Faction_NIMDOK = "Faction_NIMDOK";
    public const string Faction_Independents = "Faction_Independents";
    public const string Faction_Scavengers = "Faction_Scavengers";
    public const string Faction_Archive = "Faction_Archive";

    public const string Faction_Reputation = "Faction_Reputation";
    public const string Faction_Hostile = "Faction_Hostile";
    public const string Faction_Unfriendly = "Faction_Unfriendly";
    public const string Faction_Neutral = "Faction_Neutral";
    public const string Faction_Friendly = "Faction_Friendly";
    public const string Faction_Allied = "Faction_Allied";

    #endregion

    #region Dungeons

    public const string Dungeon_Enter = "Dungeon_Enter";
    public const string Dungeon_Exit = "Dungeon_Exit";
    public const string Dungeon_Floor = "Dungeon_Floor";
    public const string Dungeon_Clear = "Dungeon_Clear";

    public const string Dungeon_Difficulty_Easy = "Dungeon_Difficulty_Easy";
    public const string Dungeon_Difficulty_Normal = "Dungeon_Difficulty_Normal";
    public const string Dungeon_Difficulty_Hard = "Dungeon_Difficulty_Hard";
    public const string Dungeon_Difficulty_Nightmare = "Dungeon_Difficulty_Nightmare";

    #endregion

    #region Dialog

    public const string Dialog_Continue = "Dialog_Continue";
    public const string Dialog_Choice = "Dialog_Choice";
    public const string Dialog_End = "Dialog_End";

    #endregion

    #region Save/Load

    public const string Save_Title = "Save_Title";
    public const string Save_Slot = "Save_Slot";
    public const string Save_Empty = "Save_Empty";
    public const string Save_Overwrite = "Save_Overwrite";
    public const string Save_OverwriteConfirm = "Save_OverwriteConfirm";
    public const string Save_Success = "Save_Success";
    public const string Save_Failed = "Save_Failed";

    public const string Load_Title = "Load_Title";
    public const string Load_Confirm = "Load_Confirm";
    public const string Load_Success = "Load_Success";
    public const string Load_Failed = "Load_Failed";

    public const string Save_PlayTime = "Save_PlayTime";
    public const string Save_Location = "Save_Location";
    public const string Save_Chapter = "Save_Chapter";

    #endregion

    #region Tutorial

    public const string Tutorial_Movement = "Tutorial_Movement";
    public const string Tutorial_Combat = "Tutorial_Combat";
    public const string Tutorial_Inventory = "Tutorial_Inventory";
    public const string Tutorial_Quests = "Tutorial_Quests";
    public const string Tutorial_Strays = "Tutorial_Strays";
    public const string Tutorial_Map = "Tutorial_Map";
    public const string Tutorial_Skip = "Tutorial_Skip";
    public const string Tutorial_SkipConfirm = "Tutorial_SkipConfirm";

    #endregion

    #region Notifications

    public const string Notify_Autosave = "Notify_Autosave";
    public const string Notify_Achievement = "Notify_Achievement";
    public const string Notify_NewArea = "Notify_NewArea";
    public const string Notify_LowHealth = "Notify_LowHealth";
    public const string Notify_PartyFull = "Notify_PartyFull";
    public const string Notify_InventoryFull = "Notify_InventoryFull";

    #endregion

    #region Weather

    public const string Weather_Clear = "Weather_Clear";
    public const string Weather_Rain = "Weather_Rain";
    public const string Weather_Storm = "Weather_Storm";
    public const string Weather_Fog = "Weather_Fog";
    public const string Weather_DataStorm = "Weather_DataStorm";

    #endregion

    #region Time Formats

    public const string Format_PlayTimeHours = "Format_PlayTimeHours";
    public const string Format_PlayTimeMinutes = "Format_PlayTimeMinutes";
    public const string Format_Date = "Format_Date";

    #endregion

    #region Error Messages

    public const string Error_Generic = "Error_Generic";
    public const string Error_SaveFailed = "Error_SaveFailed";
    public const string Error_LoadFailed = "Error_LoadFailed";
    public const string Error_NetworkError = "Error_NetworkError";

    #endregion

    #region Confirmation Dialogs

    public const string Confirm_Title = "Confirm_Title";
    public const string Confirm_UnsavedChanges = "Confirm_UnsavedChanges";
    public const string Confirm_DeleteSave = "Confirm_DeleteSave";
    public const string Confirm_ResetSettings = "Confirm_ResetSettings";

    #endregion

    #region Tooltips

    public const string Tooltip_Locked = "Tooltip_Locked";
    public const string Tooltip_Unlocked = "Tooltip_Unlocked";
    public const string Tooltip_Requirement = "Tooltip_Requirement";

    #endregion

    #region Creature Categories

    public const string Category_Glitch = "Category_Glitch";
    public const string Category_Mechanical = "Category_Mechanical";
    public const string Category_Organic = "Category_Organic";
    public const string Category_Hybrid = "Category_Hybrid";
    public const string Category_Data = "Category_Data";
    public const string Category_Corrupted = "Category_Corrupted";

    #endregion

    #region Microchip Categories

    public const string Microchip_Protocol = "Microchip_Protocol";
    public const string Microchip_Element = "Microchip_Element";
    public const string Microchip_Augment = "Microchip_Augment";
    public const string Microchip_Driver = "Microchip_Driver";
    public const string Microchip_Daemon = "Microchip_Daemon";
    public const string Microchip_Support = "Microchip_Support";

    #endregion

    #region Status Effects

    public const string Status_Poisoned = "Status_Poisoned";
    public const string Status_Burned = "Status_Burned";
    public const string Status_Frozen = "Status_Frozen";
    public const string Status_Stunned = "Status_Stunned";
    public const string Status_Confused = "Status_Confused";
    public const string Status_Buffed = "Status_Buffed";
    public const string Status_Debuffed = "Status_Debuffed";
    public const string Status_Corrupted = "Status_Corrupted";

    #endregion
}
