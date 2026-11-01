using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// </summary>
public partial class ShopInWorld : Node3D, IPlayerInteractable
{
    /// <summary>
    ///     Config ID for a biome. This shop will provide the whistles and extra items for that biome.
    /// </summary>
    [Export]
    private uint _biome;
}