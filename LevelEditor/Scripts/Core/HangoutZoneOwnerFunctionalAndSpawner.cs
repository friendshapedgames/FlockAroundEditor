using BirdGame.Birds;
using Godot;

namespace BirdGame.Core;

/// <summary>
///     Hangout zone owner that also spawns grounded birds
/// </summary>
public partial class HangoutZoneOwnerFunctionalAndSpawner : HangoutZoneOwnerFunctional
{
    [Export]
    private bool _spawnGroundedBirds;

    [Export]
    private uint _spawnType;
}