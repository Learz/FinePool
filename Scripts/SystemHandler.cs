using Godot;
using System;

public partial class SystemHandler : Node
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_toggle_fullscreen")){
			if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed){
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			} else {
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			}
		}
	}
}
