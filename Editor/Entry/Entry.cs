using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Synaptafin.Editor.SelectionTracker {

  [Flags]
  [Serializable]
  public enum RefState {
    GameObject = 1 << 0,
    Asset = 1 << 1,
    Prefab = 1 << 2,

    // GameObject State
    Loaded = (1 << 4) | GameObject,
    Unloaded = (1 << 5) | GameObject,
    Destroyed = (1 << 6) | GameObject,
    Playing = (1 << 7) | GameObject,

    // Asset, Prefab, GameObject content Deleted
    Deleted = 1 << 8,

    // Prefab Content State
    Staged = Loaded | Prefab,
    Unstaged = Unloaded | Prefab,

    Unknown = 0,
    All = ~0,
  }

  /// <summary>
  /// normal gameobject has THE SAME GlobalObjectId in runtime and edit mode
  /// prefab instance has DIFFERENT GlobalObjectId in runtime and edit mode, which means:
  ///   - prefab instance object in runtime mode can't be restored from GlobalObjectId
  /// </summary>
  [Serializable]
  public class Entry : IEquatable<Entry> {

    [SerializeField] protected GlobalObjectId _unityId;

    [SerializeField] protected Object _cachedRef;
    // name should be cached for _cacheRef may be null
    [SerializeField] protected string _cachedName;
    [SerializeField] protected RefState _cachedRefState;
    [SerializeField] protected Texture _cachedRefIcon;

    [SerializeField] protected bool _isFavorite = false;

    public Object Ref => _cachedRef;
    public UnityEvent<bool> onFavoriteChanged = new();

    public virtual string DisplayName => _cachedName;
    public Texture Icon => _cachedRefIcon;

    public virtual RefState RefState => RefState.Unknown;

    public bool IsFavorite {
      get => _isFavorite;
      set {
        _isFavorite = value;
        onFavoriteChanged.Invoke(value);
      }
    }

    public Entry() { }

    public Entry(Object obj, GlobalObjectId id) {
      _unityId = id;
      CacheRefInfo(obj);
    }

    // Implementation of IEquatable.Equals
    public virtual bool Equals(Entry other) {
      if (other is null) {
        return false;
      }

      // check Entry itself
      if (ReferenceEquals(this, other)) {
        return true;
      }

      // Check One of the reference is not null
      // GameObjects with same GlobalObjectId can be different gameobject in play mode and edit mode
      // They are different entry even they have the same _unityId
      if (Ref != null ^ other.Ref != null) {
        return false;
      }

      // This and other both are null, check their _unityId 
      if (Ref == null && other.Ref == null) {
        return Equals(_unityId, other._unityId);
      } else {
        // check entry object reference whether is the same
        return Ref.Equals(other.Ref);
      }
    }

    // override object.Equals
    public override bool Equals(object obj) {
      return obj is Entry other && Equals(other);
    }

    public override int GetHashCode() {
      return Ref != null ? Ref.GetHashCode() : 0;
    }

    public virtual void Ping() { }

    public virtual void Open() { }

    // cache for scene switching or editor session closed
    protected void CacheRefInfo(Object obj) {
      if (obj == null) {
        return;
      }

      _cachedRef = obj;
      _cachedName = obj.name;
      _cachedRefIcon = AssetPreview.GetMiniThumbnail(obj);
    }
  }
}

