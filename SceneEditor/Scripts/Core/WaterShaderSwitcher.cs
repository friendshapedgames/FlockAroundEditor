using Godot;

namespace BirdGame.Core;

/// <summary>
///     STUBBED CLASS - NOT ALL FUNCTIONALITY WILL BE VISIBLE HERE
/// </summary>
public partial class WaterShaderSwitcher : MeshInstance3D
{
    public override void _Ready()
    {
        if (RenderingServer.GetCurrentRenderingDriverName() == "opengl3")
        {
            if (GetSurfaceOverrideMaterial(0) is ShaderMaterial material)
            {
                material.SetShaderParameter("skip_foam", true);
            }
        }
    }
}