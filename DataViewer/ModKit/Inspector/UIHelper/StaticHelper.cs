using DataViewer;
using ModKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace ToyBox.Infrastructure.UI;
public static class StaticHelper {
    internal static void LogEarly(string str) {
        Main.ModEntry.Logger.Log(str);
    }
    public static void Trace(string str) {
        if (Main.settings.LogLevel >= LogLevel.Trace)
            Main.ModEntry.Logger.Log($"[Trace] {str}");
    }
    public static void Debug(string str) {
        if (Main.settings.LogLevel >= LogLevel.Debug)
            Main.ModEntry.Logger.Log($"[Debug] {str}");
    }
    public static void Log(string str) {
        if (Main.settings.LogLevel >= LogLevel.Info) {
            Main.ModEntry.Logger.Log(str);
        }
    }
    public static void Warn(string str) {
        if (Main.settings.LogLevel >= LogLevel.Warning) {
            Main.ModEntry.Logger.Warning(str);
        }
    }
    public static void Critical(Exception ex) {
        Main.ModEntry.Logger.Critical(ex.ToString());
    }
    public static void Critical(string str, bool includeStackTrace = true) {
        Main.ModEntry.Logger.Error($"{str}:\n{(includeStackTrace ? new System.Diagnostics.StackTrace(1, true).ToString() : "")}");
    }
    public static void Error(Exception ex) {
        Main.ModEntry.Logger.Error(ex.ToString());
    }
    public static void Error(string str, int skip = 1, bool includeStackTrace = true) {
        Main.ModEntry.Logger.Error($"{str}:\n{(includeStackTrace ? new System.Diagnostics.StackTrace(skip, true).ToString() : "")}");
    }
    public static float CalculateLargestLabelSize(IEnumerable<string> items, GUIStyle? style = null) {
        style ??= GUI.skin.label;
        return items.Max(item => style.CalcSize(new(item)).x);
    }
    public static bool PressedEnterInControl(string controlName) {
        Event e = Event.current;

        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == controlName) {
            e.Use();
            return true;
        }
        return false;
    }
    public static bool ImguiCanChangeStateAtBeginning() => Event.current.type == EventType.Layout;
    public static bool ImguiCanChangeStateAtEnd() => Event.current.type == EventType.Repaint;
    public static GUILayout.HorizontalScope HorizontalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.HorizontalScope HorizontalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.VerticalScope VerticalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
    public static GUILayoutOption Width(float width) => GUILayout.Width(width);
    public static GUILayoutOption Height(float height) => GUILayout.Height(height);
    public static GUILayoutOption AutoWidth() => GUILayout.ExpandWidth(false);
    public static GUILayoutOption AutoHeight() => GUILayout.ExpandHeight(false);
    public static void Space(float pixels) => GUILayout.Space(pixels);
    public static float EffectiveWindowWidth() => 0.98f * UnityModManager.Params.WindowWidth;
}
