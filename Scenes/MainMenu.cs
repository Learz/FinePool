using Godot;
using System;

public class MainMenu : Control {
    [Export]
    NodePath focusBtn;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GetNode<Button>(focusBtn).GrabFocus();
    }

    private void _on_BtnPlay_pressed() {
        //GetNode<AnimationPlayer>("AnimationPlayer").Play("Switch_LevelSelect");
    }

    private void _on_BtnOptions_pressed() {
        GD.Print("Options Pressed!");
    }

    private void _on_BtnQuit_pressed() {
        GetTree().Quit();
    }

    public override void _Process(float delta) {
        GetNode<TextureRect>("Title").SetPosition(new Vector2(-20, 90f + Mathf.Sin((float)OS.GetTicksMsec() / 800) * 10f));
    }

}
