using Godot;
using System;

public class GameController : Spatial {
    [Export]
    NodePath[] playersPaths;
    int currentPlayerIndex = 0;
    BallController currentPlayer;

    [Export]
    NodePath cameraPath;
    Camera camera;

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("ui_reset")) {
            GetTree().ReloadCurrentScene();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (playersPaths.Length > 0) {
            currentPlayer = GetNode<BallController>(playersPaths[currentPlayerIndex]);
        }
        camera = GetNode<Camera>(cameraPath);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        camera.GlobalTranslation = new Vector3(133, 133, -133) + currentPlayer.cameraAnchor.GlobalTranslation;
    }

    public void nextPlayer() {
        currentPlayerIndex = (currentPlayerIndex + 1) % playersPaths.Length;
        currentPlayer = GetNode<BallController>(playersPaths[currentPlayerIndex]);
    }
}
