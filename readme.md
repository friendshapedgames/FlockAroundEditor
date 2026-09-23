# Flock Around Level Editor Tools & Samples

## Level Editor

### Initial Setup
To use the Level Editor, you will need some additional software:

- You will need [Godot 4.6.2](https://godotengine.org/download/archive/4.6.2-stable/) (the exact version of Godot that Flock Around uses). You will need the `.NET` version of the editor!
- You will also need [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- Download this repo, either through `git clone` or with the "Download Zip" button.

### Using the Level Editor

- Open Godot and open the `Flock Around Editor Tool` project.
- Click the Build button in the top right corner. Assuming .NET installed correctly, this should build successfully.
- Create a new Scene in the `res://Levels` folder (or duplicate an existing map already in that folder).
- Actually make your level.
    - If you are introducing new resources they must be included in the `res://EXTERNAL` folder. Ideally under a folder with your name on it (eg: `res://EXTERNAL/JimmysCoolMod/...`).

### Exporting your Level
- Press F5 in the editor tools to "run" the tool, you can also press the play button in the top right corner.
- This will open a window for the export flow.
- Provide the `res://` path of the level you want to export.
- Provide the global path to your `FlockAround.pck`, you can find this by right clicking Flock Around in your steam library, clicking `Properties` > `Installed Files` > `Browse`. 
- Click `Export`
- This will export your level (it might take a few minutes depending on how big your level is)
- This will create a `.pck` `.uids` and copy your `.tscn` into your Flock Around mods folder.
- You may want to move these resulting files into their own folder.
- If you now run Flock Around with the `-- --enablemods` launch option, your `.tscn` file should now be visible in the Config Editor.