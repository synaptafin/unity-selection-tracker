using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Synaptafin.Editor.SelectionTracker {

  [Serializable]
  public class GameObjectEntry : Entry {

    [SerializeField] protected string _cachedSceneName;
    [SerializeField] protected string _cachedScenePath;
    [SerializeField] protected Scene _cachedScene;

    [SerializeField] private bool _isPlayModeObject;

    public override string DisplayName {
      get {
        string extName = string.IsNullOrEmpty(_cachedSceneName)
          ? _cachedName
          : string.Concat(_cachedSceneName, "/", _cachedName);

        return RefState.HasFlag(RefState.Destroyed) || RefState.HasFlag(RefState.Deleted)
          ? $"<s>{extName}</s>"
          : extName;
      }
    }

    public override RefState RefState {
      get {
        if (_cachedRef == null) {
          TryRestoreAndCache();
        }
        if (!_cachedScene.isLoaded) {
          return RefState.Unloaded;
        }

        /* if (_cachedRef == null && !_cachedScene.isLoaded) { */
        /*   return _isPlayModeObject */
        /*     ? RefState.Destroyed */
        /*     : RefState.Unloaded; */
        /* } */

        if (_cachedScene.isLoaded) {
          if (_cachedRef != null) {
            return RefState.Loaded;
          }

          if (_cachedRef == null && !_isPlayModeObject) {
            return RefState.Deleted;
          }

          if (_cachedRef == null && _isPlayModeObject) {
            return RefState.Destroyed;
          }

        }
        return RefState.Unknown;
      }
    }

    public GameObjectEntry(GameObject obj, GlobalObjectId id) : base(obj, id) {
      CacheGameObjectRefInfo(obj);
    }

    public override void Ping() {
      EditorGUIUtility.PingObject(Ref);
    }

    public override void Open() {

      if (RefState.HasFlag(RefState.Loaded)) {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          EditorSceneManager.OpenScene(_cachedScenePath);
          TryRestoreAndCache();
          Selection.activeObject = Ref;
          return;
        }
      }

      if (RefState.HasFlag(RefState.Unloaded)) {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          EditorSceneManager.OpenScene(_cachedScenePath);
          TryRestoreAndCache();
          Selection.activeObject = Ref;
          return;
        }
      }
      if (RefState.HasFlag(RefState.Loaded)) {
        Ping();
      }
    }

    protected void CacheGameObjectRefInfo(GameObject obj) {
      _cachedRefState = RefState.GameObject;
      _isPlayModeObject = Application.isPlaying;
      if (obj.scene != null) {
        _cachedScene = obj.scene;
        _cachedSceneName = obj.scene.name;
        _cachedScenePath = obj.scene.path;
      }
    }

    protected bool TryRestoreFromId(out GameObject go) {

      // In play mode, do not try to restore GameObject instance by globalObjectId 
      // Same globalObjectId but different instance in play mode and edit mode
      // When GameObject in playing mode is Selected, just update the entry ref
      if (Application.isPlaying) {
        go = null;
        return false;
      }

      Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_unityId);
      if (obj is GameObject gameObject) {
        go = gameObject;
        return true;
      }
      go = null;
      return false;
    }

    protected void TryRestoreAndCache() {
      if (TryRestoreFromId(out GameObject go)) {
        CacheRefInfo(go);
        CacheGameObjectRefInfo(go);
      }
    }

  }
}
