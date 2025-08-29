// Copyright < 2021 > Narria(github user Cabarius) - License: MIT
using UnityEngine;
using UnityModManagerNet;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
//using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace DataViewer.Menus {
    public class BlueprintLoader : MonoBehaviour {
#if KM
        public delegate void LoadBlueprintsCallback(IEnumerable<BlueprintScriptableObject> blueprints);
#else
        public delegate void LoadBlueprintsCallback(IEnumerable<SimpleBlueprint> blueprints);
#endif
        LoadBlueprintsCallback callback;
#if KM
        List<BlueprintScriptableObject> blueprints;
#else
        List<SimpleBlueprint> blueprints;
#endif
        public float progress = 0;
        private static BlueprintLoader _shared;
        public static BlueprintLoader Shared {
            get {
                if (_shared == null) {
                    _shared = new GameObject().AddComponent<BlueprintLoader>();
                    UnityEngine.Object.DontDestroyOnLoad(_shared.gameObject);
                }
                return _shared;
            }
        }
        private IEnumerator coroutine;
        private void UpdateProgress(int loaded, int total) {
            if (total <= 0) {
                progress = 0.0f;
                return;
            }
            progress = (float)loaded / (float)total;
        }
        private IEnumerator LoadBlueprints() {
            int loaded = 0;
            int total = 1;
            yield return null;
#if KM
            var bpCache = ResourcesLibrary.LibraryObject;
#else
            var bpCache = ResourcesLibrary.BlueprintsCache;
#endif
            while (bpCache == null) {
                yield return null;
#if KM
                bpCache = ResourcesLibrary.LibraryObject;
#else
                bpCache = ResourcesLibrary.BlueprintsCache;
#endif
            }
#if KM
            blueprints = new List<BlueprintScriptableObject> { };
#else
            blueprints = new List<SimpleBlueprint> { };
#endif
#if KM
            var toc = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
#else
            var toc = ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints;
#endif
            while (toc == null) {
                yield return null;
#if KM
                toc = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
#else
                toc = ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints;
#endif
            }
#if Wrath
            var allGUIDs = new List<BlueprintGuid> { };
#else
            var allGUIDs = new List<string> { };
#endif
            foreach (var key in toc.Keys) {
                allGUIDs.Add(key);
            }
            total = allGUIDs.Count;
            UpdateProgress(loaded, total);
            foreach (var guid in allGUIDs) {
#if KM
                toc.TryGetValue(guid, out var bp);
#else
                var bp = bpCache.Load(guid);
#endif
                blueprints.Add(bp);
                loaded += 1;
                UpdateProgress(loaded, total);
                if (loaded % 1000 == 0) {
                    yield return null;
                }
            }
            Main.Log($"loaded {blueprints.Count} blueprints");
            this.callback(blueprints);
            yield return null;
            StopCoroutine(coroutine);
            coroutine = null;
        }
        public void Load(LoadBlueprintsCallback callback) {

            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            this.callback = callback;
            coroutine = LoadBlueprints();
            StartCoroutine(coroutine);
        }
        public bool LoadInProgress() {
            if (coroutine != null) {
                return true;
            }
            return false;
        }
    }
}