using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Synaptafin.Editor.SelectionTracker {

  [FilePath("UserSettings/SelectionTracker.asset", FilePathAttribute.Location.ProjectFolder)]
  public class EntryServicePersistence : ScriptableSingleton<EntryServicePersistence> {

    [SerializeReference]
    private List<IEntryService> _entryServices = new();
    private Dictionary<string, IEntryService> ServiceDict => _entryServices.ToDictionary(static service => service.GetType().Name);

    public List<IEntryService> EntryServices => _entryServices;

    public void OnEnable() {
      if (!TryGetService(out HistoryService _)) {
        _entryServices.Add(HistoryService.Instance);
      }
      if (!TryGetService(out MostVisitedService _)) {
        _entryServices.Add(MostVisitedService.Instance);
      }
      if (!TryGetService(out FavoritesService _)) {
        _entryServices.Add(FavoritesService.Instance);
      }
      if (!TryGetService(out SceneComponentsService _)) {
        _entryServices.Add(SceneComponentsService.Instance);
      }

      foreach (IEntryService entryService in EntryServices) {
        entryService?.OnUpdated.AddListener(OnServiceUpdate);
      }
    }

    public void RecordSelection(Entry selection) {
      ServiceDict[nameof(HistoryService)]?.RecordEntry(selection);
      ServiceDict[nameof(MostVisitedService)]?.RecordEntry(selection);
      Save(true);
    }

    public void RecordComponent(Entry entry) {
      ServiceDict[nameof(SceneComponentsService)]?.RecordEntry(entry);
    }

    public void RecordFavorites(Entry entry, bool isFavorite = false) {
      (ServiceDict[nameof(FavoritesService)] as FavoritesService)?.RecordEntry(entry, isFavorite);
      Save(true);
    }

    public void RemoveFromFavorites(Entry entry) {
      ServiceDict[nameof(FavoritesService)]?.RemoveEntry(entry);
      Save(true);
    }

    public Entry JumpToPreviousSelection() {
      return (ServiceDict[nameof(HistoryService)] as HistoryService)?.PreviousSelection();
    }

    public Entry JumpToNextSelection() {
      return (ServiceDict[nameof(HistoryService)] as HistoryService)?.NextSelection();
    }

    public void OnDisable() {
      foreach (IEntryService entryService in EntryServices) {
        entryService?.OnUpdated.RemoveListener(OnServiceUpdate);
      }
    }

    // get service by type
    public bool TryGetService<T>(out T service) where T : IEntryService {
      if (ServiceDict.TryGetValue(typeof(T).Name, out IEntryService entryService)) {
        service = (T)entryService;
        return true;
      }
      service = default;
      return false;
    }

    public void Save() {
      Save(true);
    }

    private void OnServiceUpdate() {
      Save(true);
    }
  }
}

