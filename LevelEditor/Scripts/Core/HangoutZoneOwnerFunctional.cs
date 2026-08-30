using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
///     Used by bushes, trees, and other objects that are the parent to many hangout zones.
/// </summary>
public partial class HangoutZoneOwnerFunctional : Node3D
{
    /// <summary>
    ///     Disables all hangouts below this node
    /// </summary>
    [Export]
    private bool _disableHangouts;
}