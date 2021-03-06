﻿#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif
using GameCreator.ModuleManager;

using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NJG.PUN
{
    public static class ModuleInitilize
    {
        public const string SYMBOL_STATS = "PHOTON_STATS";
        public const string SYMBOL_PHOTON = "PHOTON_MODULE";
        public const string SYMBOL_NPC = "PHOTON_RPG";
        public const string STATS_PATH = "Assets/Plugins/GameCreator/Stats";
        public const string AI_PATH = "Assets/Ninjutsu Games/GameCreator Modules/RPG";
        //public const string STATS_PATH2 = "Assets/Plugins/GameCreatorData/Stats";

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        [InitializeOnLoadMethod]
        static void Initilize()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
#if PHOTON_UNITY_NETWORKING && !PHOTON_STATS
            //var assemblies = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.Contains("Assembly-CSharp-firstpass"));
            var module = ModuleManager.GetAssetModule("com.gamecreator.module.stats");

            bool HasStats = /*(Type.GetType("GameCreator.Stats, Assembly-CSharp-firstpass") != null || assemblies.GetTypes().Any(p => p.FullName.Contains("GameCreator.Stats")))
                && */module != null && ModuleManager.IsEnabled(module.module);

            if (HasStats)
            {
                PhotonEditorUtils.AddScriptingDefineSymbolToAllBuildTargetGroups(SYMBOL_STATS);
            }
#endif

#if PHOTON_UNITY_NETWORKING && !PHOTON_RPG
            //var assemblies2 = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.Contains("Assembly-CSharp-firstpass"));
            var module2 = ModuleManager.GetAssetModule("com.ninjutsugames.modules.rpg");

            bool HasAI = /*(Type.GetType("NJG.GC.AI, Assembly-CSharp-firstpass") != null || assemblies2.GetTypes().Any(p => p.FullName.Contains("NJG.GC.AI")))
                || */module2 != null && ModuleManager.IsEnabled(module2.module);

            //Debug.LogWarning("HasAI " + HasAI+" / "+ module2);

            if (HasAI)
            {
                PhotonEditorUtils.AddScriptingDefineSymbolToAllBuildTargetGroups(SYMBOL_NPC);
            }
#endif

#if PHOTON_UNITY_NETWORKING && !PHOTON_MODULE
            //var assemblies3 = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.Contains("Assembly-CSharp-firstpass"));
            var module3 = ModuleManager.GetAssetModule("com.ninjutsugames.modules.photon");

            bool HasStats3 = /*(Type.GetType("NJG.PUN, Assembly-CSharp-firstpass") != null || assemblies3.GetTypes().Any(p => p.FullName.Contains("NJG.PUN")))
                && */module3 != null && ModuleManager.IsEnabled(module3.module);

            if (HasStats3)
            {
                PhotonEditorUtils.AddScriptingDefineSymbolToAllBuildTargetGroups(SYMBOL_PHOTON);
            }
#endif
            }
        }

        public static void CleanUpDefineSymbols()
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                    .Split(';')
                    .Select(d => d.Trim())
                    .ToList();

                List<string> newDefineSymbols = new List<string>();
                foreach (var symbol in defineSymbols)
                {
                    if (SYMBOL_STATS.Equals(symbol) || symbol.StartsWith(SYMBOL_STATS) || SYMBOL_NPC.Equals(symbol) || symbol.StartsWith(SYMBOL_NPC))
                    {
                        continue;
                    }

                    newDefineSymbols.Add(symbol);
                }

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", newDefineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Could not set clean up STATS's define symbols for build target: {0} group: {1}, {2}", target, group, e);
                }
            }
        }
    }

    public class CleanUpDefinesOnPunDelete : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            if (ModuleInitilize.STATS_PATH.Equals(assetPath) || ModuleInitilize.AI_PATH.Equals(assetPath))
            {
                ModuleInitilize.CleanUpDefineSymbols();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
