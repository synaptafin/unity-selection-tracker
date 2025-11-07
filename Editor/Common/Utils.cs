using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Synaptafin.Editor.SelectionTracker {

  [InitializeOnLoad]
  public static class Utils {

    private static readonly PreferencePersistence s_preferenceOption = PreferencePersistence.instance;

    static Utils() {
      Selection.selectionChanged += SelectionChangedCallback;
      EditorSceneManager.sceneOpened += SceneOpenedCallback;
    }

    private static void SelectionChangedCallback() {
      if (Selection.activeObject == null) {
        return;
      }

      bool isGameObject = Selection.activeObject is GameObject;

      if (isGameObject && !s_preferenceOption.GetToggleValue(Constants.RECORD_GAMEOBJECTS_KEY)) {
        return;
      }

      Entry entry = EntryFactory.Create(Selection.activeObject);
      EntryServicePersistence.instance.RecordSelection(entry);
    }

    [Shortcut("Selection Tracker/Previous Selection", KeyCode.O, ShortcutModifiers.Control)]
    public static void PreviousSelection() {
      Entry selection = EntryServicePersistence.instance.JumpToPreviousSelection();
      JumpToSelection(selection);
    }

    [Shortcut("SelectionTracker/Next Selection", KeyCode.I, ShortcutModifiers.Control)]
    public static void NextSelection() {
      Entry selection = EntryServicePersistence.instance.JumpToNextSelection();
      JumpToSelection(selection);
    }

    private static void JumpToSelection(Entry entry) {
      Object obj = entry?.Ref;
      if (obj != null) {
        Selection.activeObject = obj;
      } else {
        if (entry.RefState.HasFlag(RefState.Unloaded)) {
          entry.Ping();
        }
      }
    }


    public static void ScanAllComponentsInScene(Scene scene) {
      if (!scene.IsValid() || !scene.isLoaded) {
        Debug.LogWarning($"Scene {scene.name} is not valid or not loaded.");
        return;
      }

      Debug.Log($"Scanning components in scene: {scene.name}");

      HashSet<Type> uniqueComponentTypes = new();
      GameObject[] rootObjects = scene.GetRootGameObjects();

      foreach (GameObject rootObj in rootObjects) {
        ScanGameObjectAndChildren(rootObj, uniqueComponentTypes);
      }
      EntryServicePersistence.instance.Save();
    }

    private static void ScanGameObjectAndChildren(GameObject obj, HashSet<Type> uniqueTypes) {
      // Get all components on this GameObject
      Component[] components = obj.GetComponents<Component>();
      foreach (Component component in components) {
        if (component != null) {
          if (uniqueTypes.Add(component.GetType())) {
            EntryServicePersistence.instance.RecordComponent(EntryFactory.Create(component));
          }
        }
      }

      // Recursively scan children
      Transform transform = obj.transform;
      for (int i = 0; i < transform.childCount; i++) {
        ScanGameObjectAndChildren(transform.GetChild(i).gameObject, uniqueTypes);
      }
    }

    private static void SceneOpenedCallback(Scene scene, OpenSceneMode mode) {
      ScanAllComponentsInScene(scene);
    }

  }
}

