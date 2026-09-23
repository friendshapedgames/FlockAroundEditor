using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// </summary>
public partial class PlayPooled3DSoundAtRandomIntervals : Node3D
{
    /// <summary>
    ///     Base value - X in (X +/- Y)
    /// </summary>
    [Export]
    private float _intervalBase = 10;

    /// <summary>
    ///     Variance value - Y in (X +/- Y)
    /// </summary>
    [Export]
    private float _intervalVariance = 5;

    /// <summary>
    ///     Config ID for a Randomized sounds config.
    /// </summary>
    [Export]
    private uint _sounds;
}