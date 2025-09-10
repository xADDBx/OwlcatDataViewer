using ModKit.Utility;
using ToyBox.Infrastructure.UI;
using UnityEngine;
using static ToyBox.Infrastructure.UI.StaticHelper;

namespace DataViewer.Infrastructure.Inspector;
public static partial class InspectorUI {
    private static string ShowSearchText = "Show Search";
    private static string SearchByNameText = "Search by Name";
    private static string SearchByTypeText = "Search by Type";
    private static string SearchByValueText = "Search by Value";
    private static string SearchDepthText = "Search Depth";
    private static string StoppedDrawingEntriesToPreventUI = "Stopped drawing entries to prevent UI crash";
    private static GUIStyle m_ButtonStyle {
        get {
            field ??= new(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, stretchHeight = false };
            return field;
        }
    }
    private static readonly Dictionary<object, InspectorNode> m_CurrentlyInspecting = [];
    private static readonly HashSet<object> m_ExpandedKeys = [];
    static InspectorUI() {
        Main.OnHideGUIAction += ClearCache;
    }
    public static void ClearCache() {
        m_CurrentlyInspecting.Clear();
        m_ExpandedKeys.Clear();
        InspectorSearcher.ShouldCancel = true;
        InspectorSearcher.LastPrompt = "";
    }
    public static void InspectToggle(object key, string? title = null, object? toInspect = null, int indent = 0) {
        using (new GUILayout.VerticalScope()) {
            title ??= key.ToString();
            toInspect ??= key;
            var expanded = m_ExpandedKeys.Contains(key);
            if (ToyBox.Infrastructure.UI.UI.DisclosureToggle(ref expanded, title)) {
                if (expanded) {
                    m_ExpandedKeys.Clear();
                    m_ExpandedKeys.Add(key);
                } else {
                    m_ExpandedKeys.Remove(key);
                }
            }
            if (expanded) {
                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Space(indent);
                    Inspect(toInspect);
                }
            }
        }
    }
    private static string m_NameSearch = "";
    private static string m_TypeSearch = "";
    private static string m_ValueSearch = "";
    private static bool m_DoShowSearch = false;
    private static bool m_DoShowSettings = false;
    private static int m_SearchDepth = 2;
    private static int m_DrawnNodes;
    public static void Inspect(object? obj) {
        using (new GUILayout.VerticalScope()) {
            if (obj == null) {
                ToyBox.Infrastructure.UI.UI.Label(SharedStrings.CurrentlyInspectingText + ": " + "<null>".Cyan());
            } else {
                var valueText = "";
                try {
                    valueText = obj.ToString();
                } catch (Exception ex) {
                    Warn($"Encountered exception in Inspect -> obj.ToString():\n{ex}");
                }
                using (new GUILayout.HorizontalScope()) {
                    ToyBox.Infrastructure.UI.UI.Label(SharedStrings.CurrentlyInspectingText + ": " + valueText.Cyan());
                    GUILayout.Space(20);
                    ToyBox.Infrastructure.UI.UI.DisclosureToggle(ref m_DoShowSearch, ShowSearchText);
                    if (InspectorSearcher.IsRunning) {
                        GUILayout.Space(20);
                        ToyBox.Infrastructure.UI.UI.Label(SharedStrings.SearchInProgresText.Orange());
                        GUILayout.Space(20);
                        ToyBox.Infrastructure.UI.UI.Button(SharedStrings.CancelText.Cyan(), () => InspectorSearcher.ShouldCancel = true);
                    }
                    GUILayout.Space(20);
                    ToyBox.Infrastructure.UI.UI.DisclosureToggle(ref m_DoShowSettings, "Show Settings");
                }
                if (m_DoShowSearch) {
                    using (new GUILayout.HorizontalScope()) {
                        ToyBox.Infrastructure.UI.UI.Label(SearchDepthText + ": ", GUILayout.Width(200));
                        if (ToyBox.Infrastructure.UI.UI.ValueAdjuster(ref m_SearchDepth, 1, 0, 8, false)) {
                            if (InspectorSearcher.DidSearch) {
                                InspectorSearcher.LastPrompt = null;
                            }
                        }
                    }
                }
                if (m_DoShowSettings) {
                    using (VerticalScope()) {
                        UI.Toggle("Show Null and Empty Members", null, ref Main.settings.ToggleInspectorShowNullAndEmptyMembers, () => { }, () => { });
                        UI.Toggle("Show Static Memebers", null, ref Main.settings.ToggleInspectorShowStaticMembers, () => { }, () => { });
                        UI.Toggle("Show Fields on Enumerables", null, ref Main.settings.ToggleInspectorShowFieldsOnEnumerable, () => { }, () => { });
                        UI.Toggle("Show Compiler Generated Members", null, ref Main.settings.ToggleInspectorShowCompilerGeneratedFields, () => { }, () => { });
                        UI.Toggle("Slim Mode", null, ref Main.settings.ToggleInspectorSlimMode, () => { }, () => { });
                        using (HorizontalScope()) {
                            UI.Slider(ref Main.settings.InspectorDrawLimit, 10, 10000, 2500);
                            Space(10);
                            UI.Label("Max items drawn".Cyan());
                        }
                        using (HorizontalScope()) {
                            UI.Slider(ref Main.settings.InspectorIndentWidth, 0f, 200f, 20f);
                            Space(10);
                            UI.Label("Indent Width".Cyan());
                        }
                        using (HorizontalScope()) {
                            UI.Slider(ref Main.settings.InspectorNameFractionOfWidth, 0.01f, 0.99f, 0.3f);
                            Space(10);
                            UI.Label("Name section relative width".Cyan());
                        }
                        using (HorizontalScope()) {
                            UI.Slider(ref Main.settings.InspectorSearchBatchSize, 100, 1000000, 20000);
                            Space(10);
                            UI.Label("Searcher Batch Size (Lower numbers mean less ui lag during search but longer search time)".Cyan());
                        }
                    }
                }
                if (!m_CurrentlyInspecting.TryGetValue(obj, out InspectorNode root)) {
                    root = InspectorTraverser.BuildRoot(obj);
                    m_CurrentlyInspecting[obj] = root;
                }

                SearchBarGUI(root);
                m_DrawnNodes = 0;
                foreach (var child in root.Children) {
                    if (!InspectorSearcher.DidSearch || child.IsMatched) {
                        DrawNode(child, 1);
                    }
                }
                if (m_DrawnNodes >= Main.settings.InspectorDrawLimit) {
                    ToyBox.Infrastructure.UI.UI.Label(StoppedDrawingEntriesToPreventUI.Red().Bold());
                }
            }
        }
    }
    private static void SearchBarGUI(InspectorNode root) {
        if (m_DoShowSearch) {
            using (new GUILayout.HorizontalScope()) {
                ToyBox.Infrastructure.UI.UI.Label(SearchByNameText + ":", GUILayout.Width(200));
                ToyBox.Infrastructure.UI.UI.ActionTextField(ref m_NameSearch, "InspectorNameSearch", null, (string prompt) => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.NameSearch, root, m_SearchDepth, prompt);
                }, GUILayout.Width(200), GUILayout.MaxWidth(EffectiveWindowWidth() * 0.3f));
                GUILayout.Space(10);
                ToyBox.Infrastructure.UI.UI.Button(SharedStrings.SearchText, () => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.NameSearch, root, m_SearchDepth, m_NameSearch);
                });
            }
            using (new GUILayout.HorizontalScope()) {
                ToyBox.Infrastructure.UI.UI.Label(SearchByTypeText + ":", GUILayout.Width(200));
                ToyBox.Infrastructure.UI.UI.ActionTextField(ref m_TypeSearch, "InspectorTypeSearch", null, (string prompt) => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.TypeSearch, root, m_SearchDepth, prompt);
                }, GUILayout.Width(200), GUILayout.MaxWidth(EffectiveWindowWidth() * 0.3f));
                GUILayout.Space(10);
                ToyBox.Infrastructure.UI.UI.Button(SharedStrings.SearchText, () => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.TypeSearch, root, m_SearchDepth, m_TypeSearch);
                });
            }
            using (new GUILayout.HorizontalScope()) {
                ToyBox.Infrastructure.UI.UI.Label(SearchByValueText + ":", GUILayout.Width(200));
                ToyBox.Infrastructure.UI.UI.ActionTextField(ref m_ValueSearch, "InspectorValueSearch", null, (string prompt) => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.ValueSearch, root, m_SearchDepth, prompt);
                }, GUILayout.Width(200), GUILayout.MaxWidth(EffectiveWindowWidth() * 0.3f));
                GUILayout.Space(10);
                ToyBox.Infrastructure.UI.UI.Button(SharedStrings.SearchText, () => {
                    InspectorSearcher.StartSearch(InspectorSearcher.SearchMode.ValueSearch, root, m_SearchDepth, m_ValueSearch);
                });
            }
        }
    }
    public static void DrawNode(InspectorNode node, int indent) {
        if (m_DrawnNodes >= Main.settings.InspectorDrawLimit) {
            return;
        }
        using (new GUILayout.HorizontalScope()) {
            GUILayout.Space(indent * Main.settings.InspectorIndentWidth);
            if (!Main.settings.ToggleInspectorShowNullAndEmptyMembers && (node.IsNull || node.IsEnumerable && node.Children.Count == 0)) {
                return;
            }

            m_DrawnNodes++;

            var discWidth = ToyBox.Infrastructure.UI.UI.DisclosureGlyphWidth.Value;
            var leftOverWidth = EffectiveWindowWidth() /*- (indent * Main.settings.InspectorIndentWidth)*/ - 40 - discWidth;
            var calculatedWidth = Main.settings.InspectorNameFractionOfWidth * leftOverWidth;
            if (Main.settings.ToggleInspectorSlimMode) {
                calculatedWidth = Math.Min(calculatedWidth * leftOverWidth, node.OwnTextLength!.Value);
            }

            if (node.Children.Count > 0) {
                ToyBox.Infrastructure.UI.UI.DisclosureToggle(ref node.IsExpanded, node.LabelText, GUILayout.Width(calculatedWidth + discWidth));
            } else {
                GUILayout.Space(discWidth);
                GUILayout.Label(node.LabelText, GUILayout.Width(calculatedWidth));
            }

            if (Main.settings.ToggleInspectorSlimMode) {
                GUILayout.Space(10);
            }

            // TextArea does not parse color tags; so it needs this workaround to colour text
            var currentColor = GUI.contentColor;
            GUI.contentColor = node.ColorOverride ?? currentColor;
            GUILayout.TextArea(node.ValueText);
            GUI.contentColor = currentColor;
            if (node.AfterText != "") {
                GUILayout.Label(node.AfterText, m_ButtonStyle, GUILayout.ExpandWidth(false));
            } else {
                ToyBox.Infrastructure.UI.UI.Label("");
            }
        }
        if (InspectorSearcher.DidSearch) {
            foreach (var child in node.Children) {
                if (node.IsExpanded || child.IsMatched) {
                    DrawNode(child, indent + 1);
                }
            }
        } else {
            if (node.IsExpanded) {
                foreach (var child in node.Children) {
                    DrawNode(child, indent + 1);
                }
            }
        }
    }
}
