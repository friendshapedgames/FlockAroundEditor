using Godot;

namespace BirdGame.Birds;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
///     This node expects children that are Path3Ds, it will then create branch hangouts along each of those Path3Ds
/// </summary>
public partial class PerchBranchOwner : Node3D
{
    /// <summary>
    ///     ID of a PerchPointType Config
    /// </summary>
    [Export]
    private uint _perchPointType;
}