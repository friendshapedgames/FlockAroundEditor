using Godot;

namespace BirdGame.Birds;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// 
///     Any bird of this spawn type will spawn at a randomly selected position along this Path3D.
/// </summary>
public partial class BirdSpawnPath : Path3D
{
    [Export]
    private BirdSpawnType _birdSpawnType = BirdSpawnType.Sky;
}

public enum BirdSpawnType
{
    Sky,
    Bush,
    DistantWater
}