using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

// The Shanhai 9000/1000 dialogue choices only show the map chip options if the player doesn't
// already have the map chip item. Since rando decouples these items and locations, this
// means the Shanhai locations "break" if you happen to have the corresponding chip item.
// So we have to force them to check the AP location instead of the in-game item.

[HarmonyPatch]
class ShanhaiChips {
    private static Dictionary<string, Location> alreadyHasChipConditions = new Dictionary<string, Location> {
        {
            "A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.PRC_SHANHAI_CHIP
        },
        {
            "A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.GH_SHANHAI_CHIP
        },
        {
            "A4_S1/Room/NPC_GuideFish_A4/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.OW_SHANHAI_CHIP
        },
        {
            "A7_S1/Room/NPC_GuideFish_A7/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.CC_SHANHAI_CHIP
        },
        {
            "A10S5/Room/NPC_GuideFish_A10/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.ASP_SHANHAI_CHIP
        },
        {
            "A9_S2/Room/NPC_GuideFish_A9/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/UIOptionPanel (Providers)/RightPanel/OptionPanelBottom/UIOptions/[Talk] 晶片對話/ConditionFolder/[Condition] 沒有晶片",
            Location.EDLA_SHANHAI_CHIP
        },
    };

    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        if (__instance.name == "[Condition] 沒有晶片") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            Log.Info($"Shanhai AbstractConditionComp_get_FinalResult __result={__result} goPath={goPath}");

            if (__result == true)
                // if this condition already evaluated to true, then we don't need to change it to make the Shanhais chip locations work
                return;

            if (!alreadyHasChipConditions.ContainsKey(goPath))
                return;

            var location = alreadyHasChipConditions[goPath];
            var locName = location.ToString();
            var locationsChecked = APSaveManager.CurrentAPSaveData?.locationsChecked;
            if (locationsChecked != null && locationsChecked.ContainsKey(locName) && locationsChecked[locName])
                // if we've already checked this location, no need to force it to work again
                return;

            Log.Info($"AbstractConditionComp_get_FinalResult forcing the 'you don't already have this Shanhai's map chip' condition to pass so the player can still check {location}");
            __result = true;
        }
    }

    // Remind the player to repair FSP Shanhai 9000 if they end up talking to one of the other Shanhai first
    [HarmonyPostfix, HarmonyPatch(typeof(FlagBoolCondition), "isValid", MethodType.Getter)]
    static void FlagBoolCondition_get_isValid(FlagBoolCondition __instance, bool __result) {
        if (__instance.flagBool == null)
            return;
        var flag = __instance.flagBool.boolFlag;
        if (flag == null)
            return;
        if (flag.FinalSaveID != "9639fabc158b27646809ae12cb76f56eScriptableDataBool") // AG_S2_YiBase_[Dialogue]山海9000開機對話_IsCompleted
            return;
        var levelName = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        if (levelName == "AG_S2") // FSP
            return; // presumably all the Shanhai FSP stuff checks this variable, and we definitely don't want to do anything in there
        if (__result == true)
            return; // no need for the reminder if they already repaired it

        ToastManager.Toast("Remember that you have to repair the Shanhai 9000 in FSP" +
            "\nbefore you can ask the other Shanhai for their map chips." +
            "\n "); // blank line so this text will appear on top of the Confirm/Back button prompts, or else this is unreadable
    }
}
