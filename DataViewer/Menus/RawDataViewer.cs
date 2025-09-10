using DataViewer.Infrastructure.Inspector;
using DataViewer.Utility;
using Kingmaker;
using Kingmaker.Blueprints.Root;
#if Wrath
using Kingmaker.Globalmap.View;
#endif
using ModKit;
using ModKit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static DataViewer.Main;

namespace DataViewer.Menus
{
    public class RawDataViewer : IMenuSelectablePage
    {
        public static IEnumerable<Scene> GetAllScenes() {
            for (var i = 0; i < SceneManager.sceneCount; i++) {
                yield return SceneManager.GetSceneAt(i);
            }
        }
        private static readonly Dictionary<string, Func<object>> TARGET_LIST = new Dictionary<string, Func<object>>()
        {
            { "None", null },
            { "Game", () => Game.Instance },
            { "Player", () => Game.Instance?.Player },
            { "Characters", () => Game.Instance?.Player?.AllCharacters },
#if RT
            { "AllUnits", () => Game.Instance?.State?.AllUnits },
#else
            { "Units", () => Game.Instance?.State?.Units },
#endif
            { "States", () => Game.Instance?.State },
            { "Inventory", () => Game.Instance?.Player?.Inventory },
            { "Dialog", () => Game.Instance?.DialogController },
            { "Vendor", () => Game.Instance?.Vendor },
            { "Scene", () => SceneManager.GetActiveScene() },
#if RT
            { "RootUiContext", () => Game.Instance?.RootUiContext },
#else
            { "UI", () => Game.Instance?.UI },
            { "Static Canvas", () => Game.Instance?.UI?.Canvas?.gameObject },
#endif
            { "Quest Book", () => Game.Instance?.Player?.QuestBook },
#if KM
            { "Kingdom", () => Game.Instance?.Player?.Kingdom },
#endif
            { "Area", () => Game.Instance?.CurrentlyLoadedArea },
#if RT
            { "SectorMapController", () => Game.Instance?.SectorMapController },
            { "SectorMapController", () => Game.Instance?.SectorMapTravelController },
#elif Wrath
            { "GlobalMapController", () => Game.Instance.GlobalMapController },
            { "GlobalMapView", () => GlobalMapView.Instance },
#else
            { "GlobalMap", () => Game.Instance?.Player?.GlobalMap },
#endif
#if RT
            { "SpaceVM", () => Game.Instance.RootUiContext.SpaceVM },
#else
            { "GlobalMapUI", () => Game.Instance.UI.GlobalMapUI },
#endif
            { "BlueprintRoot", () => Kingmaker.Blueprints.Root.BlueprintRoot.Instance },
            { "Root Game Objects", () => RawDataViewer.GetAllScenes().SelectMany(s => s.GetRootGameObjects()) },
            { "Game Objects", () => UnityEngine.Object.FindObjectsOfType<GameObject>() },
            { "Unity Resources", () =>  Resources.FindObjectsOfTypeAll(typeof(GameObject)) },
       };

        private readonly string[] _targetNames = TARGET_LIST.Keys.ToArray();

        private object? m_ToInspect = null;
       
        public string Name => "Raw Data";

        public int Priority => 0;
        void ResetTree() {

            Func<object> getTarget = TARGET_LIST[_targetNames[Main.settings.selectedRawDataType]];
            m_ToInspect = getTarget();
        }
        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (ModManager == null || !ModManager.Enabled)
                return;

            try
            {
                if (m_ToInspect == null) ResetTree();

                // target selection
                UI.ActionSelectionGrid(ref Main.settings.selectedRawDataType, _targetNames, 5, (s) => {
                    ResetTree();
                });

                // tree view
                if (Main.settings.selectedRawDataType != 0)
                {
                    GUILayout.Space(10f);
                    InspectorUI.Inspect(m_ToInspect);
                }
            }
            catch (Exception e)
            {
                Main.settings.selectedRawDataType = 0;
                modEntry.Logger.Error(e.StackTrace);
                throw e;
            }
        }

    }
}
