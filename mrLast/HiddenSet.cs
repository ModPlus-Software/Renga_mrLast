namespace mrLast;

using System.Collections.Generic;
using System.Linq;
using ModPlusAPI;
using ModPlusAPI.Mvvm;
using Renga;

/// <summary>
/// Набор скрытых элементов
/// </summary>
public class HiddenSet : ObservableObject
{
    private readonly List<IModelObject> _modelObjects;

    /// <summary>
    /// Initializes a new instance of the <see cref="HiddenSet"/> class.
    /// </summary>
    /// <param name="view">Source view</param>
    public HiddenSet(IModelView view)
    {
        _modelObjects = new List<IModelObject>();
        SourceViewId = view.Id;
    }

    /// <summary>
    /// Source view id
    /// </summary>
    public int SourceViewId { get; }
        
    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Objects count
    /// </summary>
    public int Count => _modelObjects.Count;

    /// <summary>
    /// Add model object
    /// </summary>
    /// <param name="modelObjects">Model objects</param>
    public void Add(IEnumerable<IModelObject> modelObjects)
    {
        var differentType = false;
        foreach (var modelObject in modelObjects)
        {
            if (_modelObjects.Any() && !differentType)
            {
                if (_modelObjects.Last().ObjectType != modelObject.ObjectType)
                    differentType = true;
            }

            _modelObjects.Add(modelObject);
        }

        DisplayName = differentType
            ? Language.GetItem("h1")
            : ModPlus.Helpers.Localization.RengaObjectType(_modelObjects.Last().ObjectType);
    }

    /// <summary>
    /// Get object ids array
    /// </summary>
    public int[] GetIds()
    {
        return _modelObjects.Select(o => o.Id).ToArray();
    }
}