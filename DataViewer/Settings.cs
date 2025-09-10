using ModKit;
using UnityModManagerNet;

namespace DataViewer
{
    public class Settings : UnityModManager.ModSettings
    {
        public int selectedTab = 0;
        public int selectedRawDataType = 0;
        public int maxRows = 20;
        public int maxSearchDepth = 3;
        public string searchChildName = "name";
        public string searchText = "";
        public bool searchReversed = false;
        // - Inspector
        public bool ToggleInspectorShowNullAndEmptyMembers = false;
        public bool ToggleInspectorShowStaticMembers = true;
        public bool ToggleInspectorShowFieldsOnEnumerable = false;
        public bool ToggleInspectorShowCompilerGeneratedFields = true;
        public bool ToggleInspectorSlimMode = false;
        public int InspectorSearchBatchSize = 20000;
        public int InspectorDrawLimit = 4000;
        public float InspectorIndentWidth = 20f;
        public float InspectorNameFractionOfWidth = 0.3f;

        public LogLevel LogLevel = LogLevel.Info;
    }
}
