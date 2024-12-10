using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

namespace AnimationParser.Core;

/// <summary>
/// Represents a animation context that holds the state of the animation.
/// Game engines should override this class to support their internal bindings
/// and all kind of operations that can be done in the animation.
/// </summary>
public partial class AnimationContext
{
    private readonly Dictionary<string, AnimationObject> _animatedObject = [];

    public ReadOnlyDictionary<string, AnimationObject> AnimatedObjects => new(_animatedObject);

    public void DeclareObject(string name, AnimationObject value)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        if (AnimatedObjects.ContainsKey(name))
        {
            throw new Exception($"Variable '{name}' is already declared");
        }

        OnObjectAdded(name, value);
    }

    public AnimationObject? GetObject(string name)
    {
        return AnimatedObjects.TryGetValue(name, out var value) ? value : default;
    }

    /// <summary>
    /// Called by the Shift command. Should not be called unless for testing purposes.
    /// </summary>
    public void ShiftObject(string name, Direction direction)
    {
        if (!AnimatedObjects.TryGetValue(name, out var obj))
        {
            throw new Exception($"Variable '{name}' is not declared");
        }

        OnObjectShifting(name, obj, direction);
    }

    /// <summary>
    /// Called by the Place command. Should not be called unless for testing purposes.
    /// </summary>
    public void PlaceObject(string name, Vector2 position)
    {
        if (!AnimatedObjects.TryGetValue(name, out var obj))
        {
            throw new Exception($"Variable '{name}' is not declared");
        }

        Debug.Assert(obj != null, "Object should not be null");

        OnObjectPlaced(name, obj, position);
    }

    /// <summary>
    /// Called by the Erase command. Should not be called unless for testing purposes.
    /// </summary>
    public void EraseObject(string name)
    {
        if (!AnimatedObjects.TryGetValue(name, out var obj))
        {
            throw new Exception($"Variable '{name}' is not declared");
        }

        OnObjectErasingOut(name, obj);
    }

    /// <summary>
    /// Game engines should override this method to support their internal bindings.
    /// Must call base.OnObjectPlaced at the end of the animation.
    /// </summary>
    /// <param name="name">The name of the object, used for game engines to retrive their bindings.</param>
    /// <param name="obj">The object that is being placed.</param>
    /// <param name="position">The position of the object.</param>
    protected virtual void OnObjectPlaced(string name, AnimationObject obj, Vector2 position)
    {
        obj.Position = position;
    }

    /// <summary>
    /// Game engines should override this method to support their internal bindings.
    /// Must call base.OnObjectAdded at the first of the coroutine.
    /// </summary>
    /// <param name="name">The name of the object, used for game engines to retrive their bindings.</param>
    /// <param name="obj">The object that is being added.</param>
    protected virtual void OnObjectAdded(string name, AnimationObject obj)
    {
        _animatedObject[name] = obj;
    }

    /// <summary>
    /// Game engines should override this method to support their internal bindings.
    /// Must call base.OnObjectErasingOut at the end of the animation.
    /// </summary>
    /// <param name="name">The name of the object, used for game engines to retrive their bindings.</param>
    /// <param name="obj">The object that is being erased.</param>
    protected virtual void OnObjectErasingOut(string name, AnimationObject obj)
    {
        _animatedObject.Remove(name);
    }

    /// <summary>
    /// Game engines should override this method to support their internal bindings.
    /// base.OnObjectShifting is not important to be called.
    /// </summary>
    /// <param name="name">The name of the object, used for game engines to retrive their bindings.</param>
    /// <param name="obj">The object that is being shifted.</param>
    /// <param name="direction">The direction of the shift operation.</param>
    protected virtual void OnObjectShifting(string name, AnimationObject obj, Direction direction)
    {
    }
}
