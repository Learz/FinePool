using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Level : Node3D {
	private const float SPAWN_RANDOM = 5.0f;

	[Export] public PlayerControls PlayerControls;

	private List<int> playerOrder = new();
	int current_player = 0;

	[Export]
	public int CurrentPlayerId = -1;

	[Export]
	public Node Players;

	[Export]
	Camera3D Camera;
	private Boolean isCameraMoving = false;
	private Vector3 cameraOffset = new Vector3(8, 10, 8);

	public override void _Ready() {
		// We only need to spawn players on the server.
		if (!Multiplayer.IsServer())
			return;

		Multiplayer.PeerConnected += id => AddPlayer((int)id);
		Multiplayer.PeerDisconnected += id => DelPlayer((int)id);

		// Spawn already connected players
		foreach (int id in Multiplayer.GetPeers()) {
			AddPlayer(id);
		}

		// Spawn the local player unless this is a dedicated server export.
		if (!OS.HasFeature("dedicated_server")) {
			AddPlayer(1);
		}
	}

	public override void _Process(double delta) {
		if (CurrentPlayerId == -1) return;

		var playerNode = (Player)Players.GetNode(CurrentPlayerId.ToString());
		if (Camera != null && playerNode != null) {
			Camera.Position = Camera.Position.Lerp(playerNode.Position + cameraOffset, 0.2f);
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (@event.IsActionPressed("ui_focus_next")) {
			Rpc(nameof(NextPlayer));
		};
	}

	public override void _ExitTree() {
		if (!Multiplayer.IsServer())
			return;

		Multiplayer.PeerConnected -= id => AddPlayer((int)id);
		Multiplayer.PeerDisconnected -= id => DelPlayer((int)id);
	}

	public void AddPlayer(int id) {
		var playerScene = (PackedScene)GD.Load("res://Scenes/Entities/player.tscn");
		var player = (Player)playerScene.Instantiate();

		// Set player id.
		player.PlayerId = id;

		// Randomize player position.
		var rand = new System.Random();
		var angle = (float)(rand.NextDouble() * 2 * Mathf.Pi);
		var pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SPAWN_RANDOM * (float)rand.NextDouble();

		player.Position = new Vector3(pos.X, 0, pos.Y);
		player.Name = id.ToString();

		Players.AddChild(player);

		playerOrder.Add(id);
		if (CurrentPlayerId <= 0) {
			CurrentPlayerId = id;
		}
	}

	public void DelPlayer(int id) {
		if (!Players.HasNode(id.ToString()))
			return;

		Players.GetNode(id.ToString()).QueueFree();

		playerOrder.Remove(id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NextPlayer() {
		if (!Multiplayer.IsServer())
			return;
		var playerId = playerOrder[current_player];
		var playerNode = (Player)Players.GetNode(playerId.ToString());
		playerNode.isTurn = false;

		current_player = (current_player + 1) % playerOrder.Count;

		var nextPlayerId = playerOrder[current_player];
		var nextPlayerNode = (Player)Players.GetNode(nextPlayerId.ToString());
		CurrentPlayerId = nextPlayerId;

		PlayerControls.AssignPlayer(playerNode);

		GD.Print("Player List : " + playerOrder.ToString());
		GD.Print("Current Player : " + CurrentPlayerId);
		nextPlayerNode.isTurn = true;
	}
}
