using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
///     If this node has a parent that is a Label, Button, Label3D, RichTextLabel, etc, it will set the "Text" of that
///     parent to the localized string corresponding to its attached ID.
/// </summary>
public partial class LocalizedParent : Node
{
    /// <summary>
    ///     Localized string ID
    /// </summary>
    [Export]
    private uint _localizationId;
}