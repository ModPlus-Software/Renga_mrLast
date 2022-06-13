namespace mrLast;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ModPlusAPI;
using ModPlusAPI.Mvvm;
using Renga;

/// <summary>
/// Набор выбранных элементов
/// </summary>
public class SelectedSet : ObservableObject
{
    private string _displayName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedSet"/> class.
    /// </summary>
    public SelectedSet()
    {
        Ids = new ObservableCollection<int>();
        Ids.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(IsVisible));
        };
    }

    /// <summary>
    /// Element ids
    /// </summary>
    public ObservableCollection<int> Ids { get; }

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        private set
        {
            if (_displayName == value)
                return;
            _displayName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Objects count
    /// </summary>
    public int Count => Ids.Count;

    /// <summary>
    /// Is visible in UI
    /// </summary>
    public bool IsVisible => Ids.Count > 0;

    /// <summary>
    /// Add model object
    /// </summary>
    /// <param name="modelObjects">Model objects</param>
    public void Add(IList<IModelObject> modelObjects)
    {
        Ids.Clear();

        foreach (var modelObject in modelObjects)
        {
            Ids.Add(modelObject.Id);
        }

        if (modelObjects.Any())
        {
            DisplayName = modelObjects.Select(o => o.ObjectType).Distinct().Count() > 1
               ? Language.GetItem("h1")
               : ModPlus.Helpers.Localization.RengaObjectType(modelObjects.Last().ObjectType);
        }
    }
}