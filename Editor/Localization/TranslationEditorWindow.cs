using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  public class TranslationEditorWindow : EditorWindow
  {
    private TextAsset _targetFile;
    private Dictionary<string, string> _translations = new Dictionary<string, string>();
    private Dictionary<string, List<string>> _groups = new Dictionary<string, List<string>>();
    private Dictionary<string, bool> _groupFoldouts = new Dictionary<string, bool>();
    private Vector2 _scrollPosition;
    private bool _isDirty;
    private string _searchFilter = "";
    private string _newGroupName = "";
    private string _newKeyName = "";
    private int _selectedGroupIndex = 0;

    private const float KeyWidth = 280f;
    private const float RowHeight = 20f;
    private const float Padding = 4f;
    private const float GroupHeaderHeight = 24f;

    public static void Open(TextAsset file = null)
    {
      var window = GetWindow<TranslationEditorWindow>();
      window.titleContent = new GUIContent(EditorLocalization.Get("translationEditor.title"));
      window.minSize = new Vector2(700, 400);

      if (file != null)
      {
        window._targetFile = file;
        window.LoadTranslations();
      }

      window.Show();
    }

    private void OnGUI()
    {
      DrawToolbar();
      EditorGUILayout.Space(Padding);
      DrawSearchBar();
      EditorGUILayout.Space(Padding);
      DrawTranslationList();
      DrawBottomBar();
    }

    private void DrawToolbar()
    {
      using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
      {
        var newFile = (TextAsset)EditorGUILayout.ObjectField(
          _targetFile,
          typeof(TextAsset),
          false);

        if (newFile != _targetFile)
        {
          if (_isDirty && !ConfirmDiscardChanges()) return;
          _targetFile = newFile;
          LoadTranslations();
        }

        using (new EditorGUI.DisabledScope(_targetFile == null))
        {
          if (GUILayout.Button(EditorLocalization.Get("translationEditor.reload"), EditorStyles.toolbarButton, GUILayout.Width(60)))
          {
            if (!_isDirty || ConfirmDiscardChanges())
            {
              LoadTranslations();
            }
          }
        }

        using (new EditorGUI.DisabledScope(!_isDirty))
        {
          if (GUILayout.Button(EditorLocalization.Get("button.save"), EditorStyles.toolbarButton, GUILayout.Width(60)))
          {
            SaveTranslations();
          }
        }
      }
    }

    private void DrawSearchBar()
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("translationEditor.search"), GUILayout.Width(40));
        _searchFilter = EditorGUILayout.TextField(_searchFilter);
        if (GUILayout.Button("✕", GUILayout.Width(20)))
        {
          _searchFilter = "";
          GUI.FocusControl(null);
        }
      }
    }

    private void DrawTranslationList()
    {
      if (_targetFile == null)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("translationEditor.selectFile"), MessageType.Info);
        return;
      }

      _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

      var filteredGroups = GetFilteredGroups();
      var sortedGroups = filteredGroups.Keys.OrderBy(g => g).ToList();

      foreach (var group in sortedGroups)
      {
        DrawGroup(group, filteredGroups[group]);
      }

      EditorGUILayout.EndScrollView();
    }

    private Dictionary<string, List<string>> GetFilteredGroups()
    {
      if (string.IsNullOrEmpty(_searchFilter))
        return _groups;

      var result = new Dictionary<string, List<string>>();
      foreach (var kvp in _groups)
      {
        var filteredKeys = kvp.Value.Where(k =>
          k.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) ||
          (_translations.TryGetValue(k, out var v) && v.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        if (filteredKeys.Count > 0)
          result[kvp.Key] = filteredKeys;
      }
      return result;
    }

    private void DrawGroup(string groupName, List<string> keys)
    {
      if (!_groupFoldouts.ContainsKey(groupName))
        _groupFoldouts[groupName] = true;

      var headerRect = EditorGUILayout.GetControlRect(false, GroupHeaderHeight);
      var headerColor = EditorGUIUtility.isProSkin
        ? new Color(0.22f, 0.22f, 0.22f)
        : new Color(0.76f, 0.76f, 0.76f);

      EditorGUI.DrawRect(headerRect, headerColor);

      var foldoutRect = new Rect(headerRect.x + 4, headerRect.y + 2, 20, GroupHeaderHeight - 4);
      var labelText = $"{groupName} ({keys.Count})";
      var labelRect = new Rect(headerRect.x + 20, headerRect.y + 2, headerRect.width - 50, GroupHeaderHeight - 4);
      var deleteRect = new Rect(headerRect.xMax - 24, headerRect.y + 2, 20, GroupHeaderHeight - 4);

      _groupFoldouts[groupName] = EditorGUI.Foldout(foldoutRect, _groupFoldouts[groupName], "", true);
      EditorGUI.LabelField(labelRect, labelText, EditorStyles.boldLabel);

      if (GUI.Button(deleteRect, "✕", EditorStyles.miniButton))
      {
        if (EditorUtility.DisplayDialog(
          EditorLocalization.Get("translationEditor.deleteGroup"),
          string.Format(EditorLocalization.Get("translationEditor.deleteGroupConfirm"), groupName, keys.Count),
          EditorLocalization.Get("localization.delete"),
          EditorLocalization.Get("button.cancel")))
        {
          DeleteGroup(groupName);
          return;
        }
      }

      if (_groupFoldouts[groupName])
      {
        EditorGUI.indentLevel++;

        using (new EditorGUILayout.HorizontalScope())
        {
          EditorGUILayout.LabelField(EditorLocalization.Get("translationEditor.key"), EditorStyles.miniBoldLabel, GUILayout.Width(KeyWidth - 30));
          EditorGUILayout.LabelField(EditorLocalization.Get("translationEditor.value"), EditorStyles.miniBoldLabel);
        }

        foreach (var key in keys.OrderBy(k => k))
        {
          DrawTranslationRow(key);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(Padding);
      }
    }

    private void DrawTranslationRow(string key)
    {
      var currentValue = _translations.TryGetValue(key, out var val) ? val : "";
      var isMultiline = currentValue.Contains('\n');
      var lineCount = isMultiline ? currentValue.Split('\n').Length : 1;
      var rowHeight = isMultiline ? Mathf.Max(RowHeight * lineCount, RowHeight * 2) : RowHeight;

      using (new EditorGUILayout.HorizontalScope())
      {
        var shortKey = key.Contains('.') ? key.Substring(key.IndexOf('.') + 1) : key;
        EditorGUILayout.SelectableLabel(shortKey, GUILayout.Width(KeyWidth - 30), GUILayout.Height(rowHeight));

        string newValue;
        if (isMultiline)
        {
          newValue = EditorGUILayout.TextArea(currentValue, GUILayout.Height(rowHeight));
        }
        else
        {
          newValue = EditorGUILayout.TextField(currentValue, GUILayout.Height(rowHeight));
        }

        if (newValue != currentValue)
        {
          _translations[key] = newValue;
          _isDirty = true;
        }

        if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(RowHeight)))
        {
          if (EditorUtility.DisplayDialog(
            EditorLocalization.Get("translationEditor.deleteKey"),
            string.Format(EditorLocalization.Get("translationEditor.deleteKeyConfirm"), key),
            EditorLocalization.Get("localization.delete"),
            EditorLocalization.Get("button.cancel")))
          {
            DeleteKey(key);
          }
        }
      }
    }

    private void DrawBottomBar()
    {
      if (_targetFile == null) return;

      EditorGUILayout.Space(Padding);

      var groupNames = _groups.Keys.OrderBy(g => g).ToArray();
      if (_selectedGroupIndex >= groupNames.Length)
        _selectedGroupIndex = 0;

      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("translationEditor.newGroup"), GUILayout.Width(70));
        _newGroupName = EditorGUILayout.TextField(_newGroupName);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newGroupName) || _groups.ContainsKey(_newGroupName)))
        {
          if (GUILayout.Button(EditorLocalization.Get("translationEditor.addGroup"), GUILayout.Width(50)))
          {
            AddGroup(_newGroupName);
            _newGroupName = "";
            GUI.FocusControl(null);
          }
        }
      }

      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("translationEditor.addKey"), GUILayout.Width(70));
        using (new EditorGUI.DisabledScope(groupNames.Length == 0))
        {
          _selectedGroupIndex = EditorGUILayout.Popup(_selectedGroupIndex, groupNames, GUILayout.Width(120));
          _newKeyName = EditorGUILayout.TextField(_newKeyName);

          var selectedGroup = groupNames.Length > 0 ? groupNames[_selectedGroupIndex] : "";
          var fullKey = $"{selectedGroup}.{_newKeyName}";
          var isValid = groupNames.Length > 0 && !string.IsNullOrWhiteSpace(_newKeyName) && !_translations.ContainsKey(fullKey);

          using (new EditorGUI.DisabledScope(!isValid))
          {
            if (GUILayout.Button(EditorLocalization.Get("translationEditor.addKey"), GUILayout.Width(50)))
            {
              AddKey(selectedGroup, _newKeyName);
              _newKeyName = "";
              GUI.FocusControl(null);
            }
          }
        }
      }
    }

    private void AddGroup(string groupName)
    {
      if (_groups.ContainsKey(groupName)) return;
      _groups[groupName] = new List<string>();
      _groupFoldouts[groupName] = true;
      _isDirty = true;
    }

    private void DeleteGroup(string groupName)
    {
      if (!_groups.ContainsKey(groupName)) return;

      foreach (var key in _groups[groupName].ToList())
      {
        _translations.Remove(key);
      }

      _groups.Remove(groupName);
      _groupFoldouts.Remove(groupName);
      _isDirty = true;
    }

    private void AddKey(string groupName, string keyName)
    {
      var fullKey = $"{groupName}.{keyName}";
      if (_translations.ContainsKey(fullKey)) return;

      _translations[fullKey] = "";
      if (!_groups.ContainsKey(groupName))
        _groups[groupName] = new List<string>();
      _groups[groupName].Add(fullKey);
      _isDirty = true;
    }

    private void DeleteKey(string key)
    {
      _translations.Remove(key);

      var groupName = GetGroupName(key);
      if (_groups.ContainsKey(groupName))
      {
        _groups[groupName].Remove(key);
        if (_groups[groupName].Count == 0)
          _groups.Remove(groupName);
      }

      _isDirty = true;
    }

    private string GetGroupName(string key)
    {
      var dotIndex = key.IndexOf('.');
      return dotIndex > 0 ? key.Substring(0, dotIndex) : key;
    }

    private void LoadTranslations()
    {
      _translations.Clear();
      _groups.Clear();
      _groupFoldouts.Clear();
      _newKeyName = "";
      _selectedGroupIndex = 0;
      _isDirty = false;

      if (_targetFile == null) return;

      try
      {
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(_targetFile.text);
        if (dict != null)
        {
          _translations = dict;
          OrganizeIntoGroups();
        }
      }
      catch (Exception e)
      {
        EditorUtility.DisplayDialog(
          EditorLocalization.Get("translationEditor.error"),
          $"{EditorLocalization.Get("translationEditor.parseError")}\n{e.Message}",
          "OK");
      }
    }

    private void OrganizeIntoGroups()
    {
      _groups.Clear();
      foreach (var key in _translations.Keys)
      {
        var groupName = GetGroupName(key);
        if (!_groups.ContainsKey(groupName))
          _groups[groupName] = new List<string>();
        _groups[groupName].Add(key);
      }
    }

    private void SaveTranslations()
    {
      if (_targetFile == null) return;

      var path = AssetDatabase.GetAssetPath(_targetFile);
      if (string.IsNullOrEmpty(path)) return;

      try
      {
        var orderedDict = _translations.Keys.OrderBy(k => k).ToDictionary(k => k, k => _translations[k]);
        var json = JsonConvert.SerializeObject(orderedDict, Formatting.Indented);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        _isDirty = false;

        EditorUtility.DisplayDialog(
          EditorLocalization.Get("translationEditor.saved"),
          EditorLocalization.Get("translationEditor.savedMessage"),
          "OK");
      }
      catch (Exception e)
      {
        EditorUtility.DisplayDialog(
          EditorLocalization.Get("translationEditor.error"),
          $"{EditorLocalization.Get("translationEditor.saveError")}\n{e.Message}",
          "OK");
      }
    }

    private bool ConfirmDiscardChanges()
    {
      return EditorUtility.DisplayDialog(
        EditorLocalization.Get("msg.notSaved"),
        EditorLocalization.Get("translationEditor.discardChanges"),
        EditorLocalization.Get("button.yes"),
        EditorLocalization.Get("button.no"));
    }

    private void OnDestroy()
    {
      if (_isDirty)
      {
        if (EditorUtility.DisplayDialog(
          EditorLocalization.Get("msg.notSaved"),
          EditorLocalization.Get("msg.confirmSave"),
          EditorLocalization.Get("button.save"),
          EditorLocalization.Get("button.notSave")))
        {
          SaveTranslations();
        }
      }
    }
  }
}
