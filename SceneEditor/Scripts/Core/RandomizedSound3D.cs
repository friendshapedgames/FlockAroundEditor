using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUB CLASS
///     Plays a random sound from the collection of audio streams
///     Something else must tell this sound player to "Play" for it to work
/// </summary>
public partial class RandomizedSound3D : AudioStreamPlayer3D
{
    [Export]
    private AudioStream[] _audioStreams = [];
}