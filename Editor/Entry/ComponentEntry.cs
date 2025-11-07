using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Synaptafin.Editor.SelectionTracker {

  public class ComponentEntry : Entry {

    public override string DisplayName => Ref.GetType().Name;

    public ComponentEntry(Component component, GlobalObjectId id) : base(component, id) {
    }

    public override bool Equals(Entry other) {

      if (other.Ref != null && Ref != null) {
        return other.Ref.GetType() == Ref.GetType();
      }

      return false;
    }

    public override void Ping() {
      if (Ref == null) {
        Debug.LogWarning("Cannot ping: Ref is null");
        return;
      }

      Component component = Ref as Component;
      if (component == null) {
        Debug.LogWarning("Cannot ping: Ref is not a Component");
        return;
      }

      Type componentType = component.GetType();
      Scene activeScene = SceneManager.GetActiveScene();

      if (!activeScene.IsValid() || !activeScene.isLoaded) {
        Debug.LogWarning("Cannot ping: No valid active scene");
        return;
      }

      List<GameObject> objectsWithComponent = new();
      GameObject[] rootObjects = activeScene.GetRootGameObjects();

      foreach (GameObject rootObj in rootObjects) {
        FindGameObjectsWithComponent(rootObj, componentType, objectsWithComponent);
      }

      if (objectsWithComponent.Count == 0) {
        Debug.Log($"No GameObjects found with component type: {componentType.Name}");
        return;
      }

      Debug.Log($"Pinging {objectsWithComponent.Count} GameObjects with component type: {componentType.Name}");

      foreach (GameObject obj in objectsWithComponent) {
        EditorGUIUtility.PingObject(obj);
      }
    }

    public override void Open() {
      if (Ref is MonoBehaviour mb) {
        MonoScript script = MonoScript.FromMonoBehaviour(mb);
        EditorUtility.FocusProjectWindow();
        EditorGUIUtility.PingObject(script);
      } else {
        Debug.LogWarning("Built-in components cannot be opened.");
      }
    }

    private void FindGameObjectsWithComponent(GameObject obj, Type componentType, List<GameObject> results) {
      // Check if this GameObject has the component
      if (obj.GetComponent(componentType) != null) {
        results.Add(obj);
      }

      // Recursively check children
      Transform transform = obj.transform;
      for (int i = 0; i < transform.childCount; i++) {
        FindGameObjectsWithComponent(transform.GetChild(i).gameObject, componentType, results);
      }
    }
  }
}
