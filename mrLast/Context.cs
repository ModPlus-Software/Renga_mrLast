namespace mrLast;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ModPlus.Helpers;
using ModPlusAPI;
using ModPlusAPI.Mvvm;
using Renga;

/// <summary>
/// Main context
/// </summary>
public class Context : ObservableObject
{
    private readonly Application _rengaApplication;
    private readonly ISelection _selection;
    private readonly SelectionEventSource _selectionEventSource;
    private readonly List<IModelObject> _selectedObjects;
    private bool _isEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
    {
        _selectedObjects = new List<IModelObject>();
        HiddenSets = new ObservableCollection<HiddenSet>();
        SelectedSets = new ObservableCollection<SelectedSet>();
        _rengaApplication = new Application();
        _selection = _rengaApplication.Selection;
        _selectionEventSource = new SelectionEventSource(_rengaApplication.Selection);
        _selectionEventSource.ModelSelectionChanged += OnModelSelectionChanged;

        if (_rengaApplication.ActiveView is IModelView)
        {
            IsEnabled = true;
        }
    }

    /// <summary>
    /// Is plugin work enabled
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Наборы скрытых элементов
    /// </summary>
    public ObservableCollection<HiddenSet> HiddenSets { get; }

    /// <summary>
    /// Наборы выбранных элементов
    /// </summary>
    public ObservableCollection<SelectedSet> SelectedSets { get; }

    /// <summary>
    /// Set visible
    /// </summary>
    public ICommand SetVisibleCommand => new RelayCommand<HiddenSet>(set => SafeExecute.Execute(() =>
    {
        if (_rengaApplication.ActiveView is IModelView modelView && set.SourceViewId == modelView.Id)
        {
            modelView.SetObjectsVisibility(set.GetIds(), true);
            HiddenSets.Remove(set);
        }
    }));

    /// <summary>
    /// Select elements
    /// </summary>
    public ICommand SelectCommand => new RelayCommand<SelectedSet>(set => SafeExecute.Execute(() =>
    {
        var objectIds = set.Ids.ToArray();
        SelectedSets.Remove(set);
        _selection.SetSelectedObjects(objectIds);
    }));

    /// <summary>
    /// Hide elements
    /// </summary>
    public ICommand HideCommand => new RelayCommand<SelectedSet>(set => SafeExecute.Execute(() =>
    {
        if (_rengaApplication.ActiveView is IModelView modelView)
        {
            modelView.SetObjectsVisibility(set.Ids.ToArray(), false);
        }
    }));

    /// <summary>
    /// Isolate elements
    /// </summary>
    public ICommand IsolateCommand => new RelayCommand<SelectedSet>(set => SafeExecute.Execute(() =>
    {
        if (_rengaApplication.Project.Model is { } model &&
            _rengaApplication.ActiveView is IModelView modelView)
        {
            modelView.SetObjectsVisibility(model.GetObjects().GetIds(), false);
            modelView.SetObjectsVisibility(set.Ids.ToArray(), true);
        }
    }));

    /// <summary>
    /// Собрать элементы, которые были скрыты на момент запуска
    /// </summary>
    public void GetCurrentHidden()
    {
        if (_rengaApplication.ActiveView is IModelView modelView)
        {
            var project = _rengaApplication.Project;
            var modelObjectCollection = project.Model.GetObjects();
            var modelObjects = new List<IModelObject>();
            foreach (int id in modelObjectCollection.GetIds())
            {
                var modelObject = modelObjectCollection.TryGetModelObject(id, project);
                if (!modelView.IsObjectVisible(modelObject.Id))
                    modelObjects.Add(modelObject);
            }

            if (modelObjects.Any())
            {
                var set = new HiddenSet(modelView);
                set.Add(modelObjects);
                HiddenSets.Add(set);
            }
        }
    }

    /// <summary>
    /// Собрать элементы, которые были выбраны на момент запуска
    /// </summary>
    public void GetCurrentSelected()
    {
        var ids = (int[])_selection.GetSelectedObjects();
        var project = _rengaApplication.Project;
        var modelObjectCollection = project.Model.GetObjects();
        var modelObjects = ids.Select(id => modelObjectCollection.TryGetModelObject(id, project)).ToList();

        SelectedSets.Add(new SelectedSet());

        if (modelObjects.Any())
        {
            SelectedSets.First().Add(modelObjects);
        }
    }

    /// <summary>
    /// Stop events
    /// </summary>
    public void StopEvents()
    {
        _selectionEventSource?.Dispose();
    }

    private void OnModelSelectionChanged(object sender, EventArgs args) => SafeExecute.Execute(() =>
    {
        if (_rengaApplication.ActiveView is IModelView modelView)
        {
            var ids = (int[])_selection.GetSelectedObjects();

            if (ids.Length == 0 && _selectedObjects.Any())
            {
                var hidden = _selectedObjects.Where(o => !modelView.IsObjectVisible(o.Id)).ToList();
                if (hidden.Any())
                {
                    var set = new HiddenSet(modelView);
                    set.Add(hidden);
                    HiddenSets.Insert(0, set);
                }

                SelectedSets.Insert(0, new SelectedSet());
            }
            else
            {
                _selectedObjects.Clear();
                var project = _rengaApplication.Project;
                var modelObjectCollection = project.Model.GetObjects();
                foreach (var id in ids)
                {
                    _selectedObjects.Add(modelObjectCollection.TryGetModelObject(id, project));
                }

                if (!SelectedSets.First().Ids.Intersect(_selectedObjects.Select(o => o.Id)).Any())
                    SelectedSets.Insert(0, new SelectedSet());

                SelectedSets.First().Add(_selectedObjects);
            }
        }
    });
}