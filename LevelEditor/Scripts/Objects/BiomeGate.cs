using BirdGame.Core;
using Godot;

namespace BirdGame.Objects;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// </summary>
public partial class BiomeGate : Node3D, IPlayerInteractable
{
    /// <summary>
    ///     Config ID for the Biome this gate unlocks.
    /// </summary>
    [Export]
    private uint _biomeEnum;
}