﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ActionGame;
using Config;
using HarmonyLib;
using UnityEngine.UI;

namespace KoikatuGameplayMod
{
    internal static class ClassCharaLimitUnlockHooks
    {
        private const int UnlockedMaxCharacters = 99;

        public static void ApplyHooks(Harmony instance)
        {
            instance.PatchAll(typeof(ClassCharaLimitUnlockHooks));
            var transpiler = new HarmonyMethod(typeof(ClassCharaLimitUnlockHooks), nameof(NPCLoadAllUnlock));
            PatchNPCLoadAll(instance, transpiler);
        }

        private static void PatchNPCLoadAll(Harmony instance, HarmonyMethod transpiler)
        {
            var t = typeof(ActionScene).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single(x => x.Name.StartsWith("<NPCLoadAll>c__Iterator"));
            var m = t.GetMethod("MoveNext");
            instance.Patch(m, null, null, transpiler);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClassRoomList), "Start")]
        public static void ClassRoomListUnlock(ClassRoomList __instance)
        {
            var f = typeof(ClassRoomList).GetField("sldAttendanceNum", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var sld = (Slider) f.GetValue(__instance);
            sld.maxValue = UnlockedMaxCharacters;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EtceteraSetting), "Init")]
        public static void EtceteraSettingUnlock(EtceteraSetting __instance)
        {
            var f = typeof(EtceteraSetting).GetField("maxCharaNumSlider", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var sld = (Slider) f.GetValue(__instance);
            sld.maxValue = UnlockedMaxCharacters;
        }

        public static IEnumerable<CodeInstruction> NPCLoadAllUnlock(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_S)
                {
                    if (((sbyte) 0x26).Equals(instruction.operand))
                        instruction.operand = UnlockedMaxCharacters;
                }
                yield return instruction;
            }
        }
    }
}
