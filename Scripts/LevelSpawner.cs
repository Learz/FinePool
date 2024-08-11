using Godot;
using System;

public partial class LevelSpawner : MultiplayerSpawner
{
	[Export(PropertyHint.Dir)]
	public string LevelDirectory { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScanForLevels();
	}

	private void ScanForLevels() {
		using var dir = DirAccess.Open(LevelDirectory);
		if (dir != null) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "") {
				if (dir.CurrentIsDir()) {
					GD.Print($"Found directory: {fileName}");
				} else {
					AddSpawnableScene(LevelDirectory + fileName);
					GD.Print($"Found file: {fileName}");
				}
				fileName = dir.GetNext();
			}
		} else {
			GD.Print("An error occurred when trying to access the path.");
		}
	}
}
