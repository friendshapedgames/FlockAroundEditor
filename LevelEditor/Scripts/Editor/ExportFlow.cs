using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using FileAccess = Godot.FileAccess;

namespace FlockAroundLevelEditor.Scripts.Editor;

/// <summary>
///     This is a one file script that represents the entire export flow for the editor with no dependencies.
///     I promise I don't normally write code like this.
/// </summary>
public partial class ExportFlow : Node
{
    private RichTextLabel _consoleOutput = null!;
    private CustomLogger _customLogger = new();
    private bool _isWorking;
    private string? _levelScenePath;
    private string? _pckPath;

    public override void _Ready()
    {
        _pckPath = ReadTextResourceFileLines("user://pckpath");
        _levelScenePath = ReadTextResourceFileLines("user://scenepath");
        OS.AddLogger(_customLogger);
        _customLogger.Message += Print;

        _consoleOutput = GetNode<RichTextLabel>("Control/PanelContainer/ConsoleOutput");
        var exportButton = GetNode<Button>("Control/BottomButtons/ExportButton");
        var levelPathLineEdit = GetNode<LineEdit>("Control/HBox/LevelPathLineEdit");
        var pckLineEdit = GetNode<LineEdit>("Control/HBox2/LevelPathLineEdit");
        var openFilesButton = GetNode<Button>("Control/BottomButtons/OpenFilesButton");
        var clearButton = GetNode<Button>("Control/BottomButtons/ClearButton");

        if (OS.GetName() == "macOS")
        {
            pckLineEdit.PlaceholderText =
                "~/Library/Application Support/Steam/steamapps/common/FlockAround/FlockAround.app/Contents/Resources/Flock Around.pck";
        }

        pckLineEdit.TextChanged += OnPckPathChanged;
        exportButton.Pressed += OnExportPressed;
        openFilesButton.Pressed += OnOpenFilesPressed;
        levelPathLineEdit.TextChanged += OnLevelPathChanged;
        clearButton.Pressed += OnClearPressed;

        pckLineEdit.Text = _pckPath;
        levelPathLineEdit.Text = _levelScenePath;

        _consoleOutput.Clear();
    }

    private void OnPckPathChanged(string newText)
    {
        _pckPath = newText;
    }

    private void OnClearPressed()
    {
        _consoleOutput.Clear();
    }

    private void OnLevelPathChanged(string newText)
    {
        _levelScenePath = newText;
    }

    private void OnOpenFilesPressed()
    {
        OS.ShellOpen(GlobalModPath());
    }

    private void WriteTextResourceFileLines(string path, string newText)
    {
        using var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        fileAccess.StoreString(newText);
    }

    public string? ReadTextResourceFileLines(string path)
    {
        using var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (fileAccess == null)
        {
            return null;
        }

        return fileAccess.GetAsText();
    }

    private void OnExportPressed()
    {
        if (_isWorking)
        {
            return;
        }

        _isWorking = true;
        Task.Run(() =>
        {
            DoExport();
            _isWorking = false;
        });
    }

    private void DoExport()
    {
        var hasError = false;
        GD.Print($"Export started {DateTime.Now}");

        if (string.IsNullOrWhiteSpace(_levelScenePath))
        {
            PrintError("Missing level scene path, please provide one.");
            return;
        }

        if (!ResourceLoader.Exists(_levelScenePath))
        {
            PrintError(
                $"{_levelScenePath} does not exist, please make sure it's spelled correctly (hint CTRL+SHIFT+C in Godot copies the path of a resource)");
            return;
        }

        WriteTextResourceFileLines("user://scenepath", _levelScenePath);

        if (string.IsNullOrWhiteSpace(_pckPath))
        {
            PrintError(
                "Please provide the PCK path to your FlockAround.pck, this will be located in your steam install directory");

            if (OS.GetName() == "macOS")
            {
                PrintError("hint: on macOS you will need to open the .app as a directory with right click.");
            }

            return;
        }

        WriteTextResourceFileLines("user://pckpath", _pckPath);

        if (!File.Exists(_pckPath))
        {
            PrintError($"File not found: {_pckPath}");
            return;
        }

        GD.Print("Building local resource cache...");
        // We really want something like a Trie here but a list is good enough.
        // If we have "res://Path/To/Thing.png" we want to find "res://Path/To/Thing.png.import" (or .uid, or whatever other suffix) 
        var resourceCache = new List<string>();
        foreach (var item in ListFilesRecursive("res://"))
        {
            resourceCache.Add(item);
        }

        GD.Print("Loading game PCK...");
        using var pckReader = new PckReader(_pckPath, false);

        var engineVersionDictionary = Engine.GetVersionInfo();
        var runningEngineVersion = new Version(engineVersionDictionary["major"].AsInt32(),
            engineVersionDictionary["minor"].AsInt32(), engineVersionDictionary["patch"].AsInt32());
        if (pckReader.GodotVersion != runningEngineVersion)
        {
            PrintWarning(
                $"The game PCK was built with engine version {pckReader.GodotVersion}, you are running this tool out of version {runningEngineVersion}, something might not work.");
        }

        var vanillaGameFiles = new HashSet<string>();

        if (!pckReader.FileExistsExactly("VERSION.json"))
        {
            GD.PrintErr("Could not find VERSION.json in the game pck.");
            return;
        }

        var versionJson = pckReader.ReadFileString("VERSION.json");
        var version = JsonSerializer.Deserialize<VersionJson>(versionJson) ?? new VersionJson();

        GD.Print($"PCK Game Version: {version.SemVer} {version.Branch} {version.Commit.Substring(0, 7)}");

        foreach (var fileName in pckReader.ListFiles())
        {
            if (fileName.StartsWith(".godot"))
            {
                continue;
            }

            var finalFileName = fileName;

            finalFileName = RemoveSuffix(finalFileName, ".remap");
            finalFileName = RemoveSuffix(finalFileName, ".import");

            vanillaGameFiles.Add("res://" + finalFileName);
        }

        var levelScene = ResourceLoader.Load(_levelScenePath) as PackedScene;

        if (levelScene == null)
        {
            PrintError($"{_levelScenePath} is not a scene.");
            return;
        }

        var modPath = GlobalModPath();
        Directory.CreateDirectory(modPath);

        var pck = new PckPacker();

        GD.Print($"Level scene path: {_levelScenePath}");

        // Convert res://Some/Directory/MyCoolLevel.tscn -> MyCoolLevel
        var levelName = string.Join(".", _levelScenePath.Split("/").Last().Split(".").SkipLast(1));

        var pckPath = Path.Join(modPath, $"{levelName}_assets.pck");
        GD.Print($"Writing to pck: {pckPath}");
        pck.PckStart(pckPath);

        var fileToUid = new System.Collections.Generic.Dictionary<string, string>();

        var externalDirectory = "res://EXTERNAL/";
        foreach (var dependencyPath in GetDependenciesRecursive(_levelScenePath))
        {
            if (!dependencyPath.StartsWith(externalDirectory))
            {
                if (vanillaGameFiles.Contains(dependencyPath))
                {
                    // GD.Print($"[color=gray]Skipping dependency {dependencyPath} (game already has this)[/color]");
                }
                else
                {
                    PrintError(
                        $"Dependency {dependencyPath} is not in {externalDirectory} and will NOT be exported, move it into {externalDirectory} if you want it included.");
                    hasError = true;
                }
            }
            else
            {
                GD.Print($"Found dependency: {dependencyPath}");
                AddUid(fileToUid, dependencyPath);
                var wasFound = false;

                // If our path is something like res://Path/To/File.png
                // we want to find both res://Path/To/File.png and res://Path/To/File.png.import
                foreach (var item in resourceCache.Where(item => item.StartsWith(dependencyPath)))
                {
                    AddPckFile(pck, item);

                    if (item.EndsWith(".import"))
                    {
                        var config = new ConfigFile();
                        var err = config.Load(item);
                        if (err != Error.Ok)
                        {
                            PrintError($"Failed to parse file: {item}");
                            return;
                        }

                        var sectionName = "remap";
                        foreach (var key in config.GetSectionKeys(sectionName))
                        {
                            // kind of sloppy, but we're looking for things like `path` and `path.s3tc` and `path.etc2` 
                            if (key.StartsWith("path"))
                            {
                                var itemReferencedInImportFile = config.GetValue(sectionName, key).AsString();

                                if (resourceCache.Contains(itemReferencedInImportFile))
                                {
                                    AddPckFile(pck, itemReferencedInImportFile);
                                }
                            }
                        }
                    }

                    wasFound = true;
                }

                if (!wasFound)
                {
                    PrintError($"Could not find local dependency {dependencyPath})");
                    PrintError("Hint: Capitalization might be wrong?");
                    return;
                }
            }
        }

        if (hasError)
        {
            PrintError("Export stopped due to errors.");
            return;
        }

        pck.Flush(true);

        GD.Print($"Copying level scene: {levelName}.tscn");
        var levelBytes = File.ReadAllBytes(ProjectSettings.GlobalizePath(_levelScenePath));
        File.WriteAllBytes(Path.Join(modPath, $"{levelName}.tscn"), levelBytes);

        GD.Print($"Writing uids file: {levelName}.uids");
        File.WriteAllText(Path.Join(modPath, $"{levelName}.uids"), string.Join("\n", fileToUid.Select(a =>
            $"{a.Value} {a.Key}")));

        GD.Print($"[color=lime]Export to {pckPath} finished {DateTime.Now}[/color]");

        if (_customLogger.ConsumeErrorFlag())
        {
            PrintWarning("Godot logged errors during your export, you may want to try again.");
        }
    }

    private static void AddUid(System.Collections.Generic.Dictionary<string, string> fileToUid, string path)
    {
        AddUidPath(fileToUid, path, ResourceUid.Singleton.IdToText(ResourceLoader.GetResourceUid(path)));
    }

    private static void AddUidPath(System.Collections.Generic.Dictionary<string, string> fileToUid,
        string dependencyPath, string uid)
    {
        GD.Print($"[color=lightblue]+ Adding UID mapping: {uid} <=> {dependencyPath}[/color]");
        fileToUid[dependencyPath] = uid;
    }

    private void AddPckFile(PckPacker pck, string item)
    {
        GD.Print($"[color=lightgreen]+ Adding dependency to PCK: {item}[/color]");
        pck.AddFile(item, item);
    }

    private IEnumerable<string> ListFilesRecursive(string path)
    {
        using var directory = DirAccess.Open(path);
        directory.ListDirBegin();
        var fileName = directory.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!directory.CurrentIsDir())
            {
                var filePath = path.PathJoin(fileName);
                yield return filePath;
            }
            else
            {
                if (fileName != "." && fileName != "..")
                {
                    var subPath = path.PathJoin(fileName);
                    foreach (var subFile in ListFilesRecursive(subPath))
                    {
                        yield return subFile;
                    }
                }
            }

            fileName = directory.GetNext();
        }
    }

    private static string RemoveSuffix(string finalFileName, string suffix)
    {
        if (finalFileName.EndsWith(suffix))
        {
            finalFileName = finalFileName.Substring(0, finalFileName.Length - suffix.Length);
        }

        return finalFileName;
    }


    private static void PrintWarning(string message)
    {
        GD.Print($"[color=yellow]WARNING: {message}[/color]");
    }

    private static void PrintError(string message)
    {
        GD.Print($"[color=pink]{message}[/color]");
    }

    private static string GlobalModPath()
    {
        var userDirectory = new DirectoryInfo(ProjectSettings.GlobalizePath("user://"));
        var modPath = Path.Join(userDirectory.Parent!.FullName, "SecretPlan", "FlockAround", "Mods");
        return modPath;
    }

    private void Print(string text)
    {
        _consoleOutput.CallDeferred(RichTextLabel.MethodName.AppendText, text);
    }

    public static IEnumerable<string> GetDependenciesRecursive(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            PrintError($"Resource does not exist: {path}");
        }

        var seenPaths = new HashSet<string>();
        var pathsToExplore = new Queue<string>();
        seenPaths.Add(path);
        pathsToExplore.Enqueue(path);

        while (pathsToExplore.Count > 0)
        {
            var currentPath = pathsToExplore.Dequeue();
            foreach (var item in ResourceLoader.GetDependencies(currentPath))
            {
                var newPath = item.Split("::").Last();

                if (!seenPaths.Contains(newPath))
                {
                    yield return newPath;

                    if (ResourceLoader.Exists(newPath))
                    {
                        pathsToExplore.Enqueue(newPath);
                    }
                    else
                    {
                        PrintError($"Missing dependency: {newPath}");
                    }
                }

                seenPaths.Add(newPath);
            }
        }
    }

    public static IEnumerable<string> ListResourceDirectoryRecursive(string path, bool ignoreDotGodot)
    {
        foreach (var entry in ResourceLoader.ListDirectory(path))
        {
            if (ignoreDotGodot && entry == "res://.godot/")
            {
                continue;
            }

            var fullPathStringBuilder = new StringBuilder();
            if (path.EndsWith('/'))
            {
                fullPathStringBuilder
                    .Append(path)
                    .Append(entry);
            }
            else
            {
                fullPathStringBuilder
                    .Append(path)
                    .Append('/')
                    .Append(entry);
            }

            var fullPath = fullPathStringBuilder.ToString();

            if (entry.EndsWith('/'))
            {
                foreach (var file in ListResourceDirectoryRecursive(fullPath.TrimEnd('/'), ignoreDotGodot))
                {
                    yield return file;
                }
            }
            else
            {
                yield return fullPath;
            }
        }
    }

    private void PrettyPrintBytes(byte[] bytes)
    {
        var stringBuilder = new StringBuilder();
        foreach (var fileByte in bytes)
        {
            var asChar = (char)fileByte;

            if (char.IsControl(asChar))
            {
                if (asChar == '\n')
                {
                    stringBuilder.Append("[color=orange]\\n[/color]");
                }
                else
                {
                    stringBuilder.Append($"[color=orange][{fileByte:X}][/color]");
                }
            }
            else
            {
                stringBuilder.Append(asChar);
            }
        }

        GD.Print(stringBuilder.ToString());
    }

    public partial class CustomLogger : Logger
    {
        private bool _errorFlag;

        public override void _LogMessage(string message, bool error)
        {
            Message?.Invoke(message);
        }

        public event Action<string>? Message;

        public override void _LogError(string function, string file, int line, string code, string rationale,
            bool editorNotify, int errorType,
            Array<ScriptBacktrace> scriptBacktraces)
        {
            var errorTypeName = errorType switch
            {
                0 => "ERROR",
                1 => "WARNING",
                2 => "SCRIPT",
                3 => "SHADER",
                _ => "UNKNOWN"
            };

            _errorFlag = true;

            Message?.Invoke(
                $"[color=pink]Caught {errorTypeName} at {file}:{line}\nMessage: {code}\nRationale: {rationale}[/color]\n");
            Message?.Invoke("Error Backtrace:\n");
            foreach (var item in scriptBacktraces)
            {
                if (item.IsEmpty())
                {
                    continue;
                }

                Message?.Invoke(item.Format() + "\n");
            }
        }

        public bool ConsumeErrorFlag()
        {
            if (_errorFlag)
            {
                return true;
            }

            _errorFlag = false;
            return false;
        }
    }

    public class VersionJson
    {
        [JsonPropertyName("semver")]
        public string SemVer { get; set; } = "0.0.0";

        [JsonPropertyName("commit")]
        public string Commit { get; set; } = "0000000000000000000000000000000000000000";

        [JsonPropertyName("branch")]
        public string Branch { get; set; } = "unknown";
    }
}