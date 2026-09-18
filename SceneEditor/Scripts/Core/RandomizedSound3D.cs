using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
///     Plays one of a random sound
/// </summary>
public partial class RandomizedSound3D : AudioStreamPlayer3D
{
    [Export]
    private AudioStream[] _audioStreams = [];
}