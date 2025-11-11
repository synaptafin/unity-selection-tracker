using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synaptafin.Editor.SelectionTracker {
  public static class EntryFactory {
    public static Entry Create(Object obj) {
      GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
      if (obj is GameObject go) {

        // prefab instance is treated as gameobject
        if (id.identifierType == 2) {
          return new GameObjectEntry(go, id);
        }

        // identifierType=2 doesn't cover DDOL objects
        if (go.scene == GetDontDestroyOnLoadScene()) {
          return new GameObjectEntry(go, id);
        }

        // prefab content is gameobject in prefab edit mode
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null) {
          return new PrefabContentEntry(go, id, prefabStage);
        }
      }

      // normal asset or prefab asset
      if (id.identifierType is 1 or 3) {
        return new NormalAssetEntry(obj, id);
      }

      if (obj is Component component) {
        return new ComponentEntry(component, id);
      }

      return null;
    }

    private static Scene GetDontDestroyOnLoadScene() {
      GameObject temp = new("TempForDDOL");
      Object.DontDestroyOnLoad(temp);
      Scene dontDestroyOnLoadScene = temp.scene;
      Object.DestroyImmediate(temp);
      return dontDestroyOnLoadScene;
    }
  }

}

