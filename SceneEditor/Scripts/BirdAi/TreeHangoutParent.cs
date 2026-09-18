using Godot;

namespace BirdGame.BirdAi;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// </summary>
public partial class TreeHangoutParent : Node3D
{
    /// <summary>
    ///     If enabled, birds that perch on tree trunks (eg: woodpeckers) will not perch on the trunk of this tree.
    /// </summary>
    [Export]
    private bool _preventPerchingOnTrunk;
}