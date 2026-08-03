using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
  [InitializeOnLoad]
  public static class ScreenUISceneOverlay
  {
    private static GUIStyle _frontStyle;
    private static GUIStyle _backStyle;

    static ScreenUISceneOverlay()
    {
      SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void EnsureStyles()
    {
      if (_frontStyle != null) return;

      _frontStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 40,
        alignment = TextAnchor.MiddleCenter,
      };
      _frontStyle.normal.textColor = new Color(0.00f, 0.54f, 0.48f, 0.88f);

      _backStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 40,
        alignment = TextAnchor.MiddleCenter,
      };
      _backStyle.normal.textColor = new Color(0.75f, 0.22f, 0.17f, 0.88f);
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
      var gameObjects = Selection.gameObjects;
      if (gameObjects == null || gameObjects.Length == 0) return;

      EnsureStyles();

      var drawn = new HashSet<int>();

      foreach (var go in gameObjects)
      {
        if (go == null) continue;

        var uiController = go.GetComponent<UIController>()
                           ?? go.GetComponentInChildren<UIController>(true);
        if (uiController == null) continue;

        int id = uiController.GetInstanceID();
        if (!drawn.Add(id)) continue;

        DrawLabel(uiController.transform, sceneView);
      }
    }

    private static void DrawLabel(Transform t, SceneView sceneView)
    {
      Vector3 objToCamera = (sceneView.camera.transform.position - t.position).normalized;
      float dot = Vector3.Dot(objToCamera, t.forward);

      bool isFront = dot < 0;

      string label = EditorLocalization.Get(isFront ? "sceneOverlay.front" : "sceneOverlay.back");
      Handles.Label(t.position, label, isFront ? _frontStyle : _backStyle);
    }
  }
}
