using Godot;

/// <summary>
///     STUBBED CLASS: This is NOT the real source code for this class.
///     This is just enough code to provide exactly enough API surface area for Godot.
/// </summary>
public partial class ControllerIconTexture : Texture2D
{
    [Export]
    public string path { get; set; } = "";

    [Export]
    public EShowMode show_mode { get; set; }

    [Export]
    public Devices force_controller_icon_style { get; set; }

    [Export]
    public EInputType force_type { get; set; }

    [Export]
    public EForceDevice force_device { get; set; } = EForceDevice.ANY;

    [Export]
    public LabelSettings custom_label_settings { get; set; } = null!;

    public override int _GetWidth()
    {
        return 100;
    }

    public override int _GetHeight()
    {
        return 100;
    }
}


public enum EInputType
{
    NONE,
    KEYBOARD_MOUSE, // The input is from the keyboard and/or mouse.
    CONTROLLER // The input is from a controller.
}

public enum EPathType
{
    INPUT_ACTION, // The path is an input action.
    JOYPAD_PATH, // The path is a generic joypad path.
    SPECIFIC_PATH // The path is a specific path.
}

public enum EShowMode
{
    ANY, // Icon will be display on any input method.
    KEYBOARD_MOUSE, // Icon will be display only when the keyboard/mouse is being used.
    CONTROLLER // Icon will be display only when a controller is being used.
}

public enum Devices
{
    NONE = -1,
    LUNA,
    OUYA,
    PS3,
    PS4,
    PS5,
    STADIA,
    STEAM,
    SWITCH,
    JOYCON,
    XBOX360,
    XBOXONE,
    XBOXSERIES,
    STEAM_DECK
}

public enum EForceDevice
{
    DEVICE_0,
    DEVICE_1,
    DEVICE_2,
    DEVICE_3,
    DEVICE_4,
    DEVICE_5,
    DEVICE_6,
    DEVICE_7,
    DEVICE_8,
    DEVICE_9,
    DEVICE_10,
    DEVICE_11,
    DEVICE_12,
    DEVICE_13,
    DEVICE_14,
    DEVICE_15,
    ANY // No device will be forced
}