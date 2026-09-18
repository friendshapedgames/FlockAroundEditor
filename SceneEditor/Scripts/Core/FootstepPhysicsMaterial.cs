using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
///     If a collider has one of these as a physics material, your feet will make the corresponding sounds for the attached
///     footstep config when you walk on top of it.
/// </summary>
[GlobalClass]
public partial class FootstepPhysicsMaterial : PhysicsMaterial
{
    /// <summary>
    ///     ID of a FootstepSounds Config
    /// </summary>
    [Export]
    private uint _footstepSounds;
}