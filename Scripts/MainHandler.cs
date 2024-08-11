using Godot;
using System;

public partial class MainHandler : Node {
	private const int PORT = 4433;

	[ExportGroup("Controls")]
	[Export] PlayerControls PlayerControls;

	[ExportGroup("Nodes")]
	[Export] Node Level;
	Level CurrentLevel {
		get {
			return Level.GetChildOrNull<Level>(0);
		}
	}
	[Export] MultiplayerSpawner LevelSpawner;

	[ExportGroup("UI")]
	[Export] Control MainMenu;
	[Export] Control PlayerUi;
	[Export] Control Lobby;
	[Export] TextureRect Logo;
	[Export] Control MenuOptions;
	[Export] Control PlayerList;

	[ExportGroup("Buttons")]
	[Export] Button HostButton;
	[Export] Button JoinButton;
	[Export] Button StartButton;
	[Export] Button LeaveButton;

	// ---- Overrides ----

	public override void _Ready() {
		DisplayServer.WindowSetMinSize(new Vector2I(800, 600));
		// Start paused
		//GetTree().Paused = true;

		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;

		HostButton.Pressed += OnHostPressed;
		JoinButton.Pressed += OnConnectPressed;
		StartButton.Pressed += StartGame;
		LeaveButton.Pressed += OnLeaveLobbyButtonPressed;

		// Automatically start the server in headless mode.
		if (DisplayServer.GetName() == "headless") {
			GD.Print("Automatically starting dedicated server");
			CallDeferred(nameof(OnHostPressed));
		}
	}

	public override void _Input(InputEvent @event) {
		if (!Multiplayer.IsServer())
			return;

		if (@event.IsActionPressed("dev_n")) PlayerControls.AssignPlayer(null);

		if (@event.IsAction("ui_home") && Input.IsActionJustPressed("ui_home")) {
			CallDeferred(nameof(ChangeLevel), GD.Load<PackedScene>("res://Scenes/Levels/level.tscn"));
		}
	}

	// ---- Signals ----

	private void OnHostPressed() {
		// Start as server
		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(PORT);
		if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected) {
			OS.Alert("Failed to start multiplayer server");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		EnterLobby();
	}

	private void OnConnectPressed() {
		// Start as client
		var txt = (string)GetNode("UI/MainMenu/Margin/VBoxContainer/Options/MenuOptions/Remote").Get("text");
		if (string.IsNullOrEmpty(txt)) {
			OS.Alert("Need a remote to connect to.");
			return;
		}
		var peer = new ENetMultiplayerPeer();
		peer.CreateClient(txt, PORT);
		if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected) {
			OS.Alert("Failed to start multiplayer client");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		EnterLobby();
	}

	private void OnLeaveLobbyButtonPressed() {
		LeaveLobby();
		Multiplayer.MultiplayerPeer = null;
	}

	private void PeerConnected(long id) {
		AddPlayerToLobby(id);
	}

	private void PeerDisconnected(long id) {
		RemovePlayerFromLobby(id);
	}

	private void OnPlayerNameLineEditTextSubmitted(string newText) {
		GD.Print("Renaming player " + Multiplayer.GetUniqueId().ToString() + " to " + newText);
		if (!PlayerList.HasNode(Multiplayer.GetUniqueId().ToString())) {
			GD.Print("Player " + Multiplayer.GetUniqueId().ToString() + " not found in " + PlayerList.GetChildren().ToString());
			return;
		}

		var lobbyPlayer = (LobbyPlayer)PlayerList.GetNode(Multiplayer.GetUniqueId().ToString());
		// Assuming `ChangeName` is a method of `lobbyPlayer`.
		lobbyPlayer.ChangeName(newText);
	}

	// ---- Functions ----

	private void EnterLobby() {
		MenuOptions.Hide();
		foreach (var playerID in Multiplayer.GetPeers()) {
			AddPlayerToLobby(playerID);
		}
		if (Multiplayer.IsServer() && !OS.HasFeature("dedicated_server")) {
			AddPlayerToLobby(1);
		}
		Logo.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
		Lobby.Show();
	}

	private void LeaveLobby() {
		MenuOptions.Show();
		foreach (Node lobbyPlayer in PlayerList.GetChildren()) {
			lobbyPlayer.QueueFree();
		}
		Logo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		Lobby.Hide();
	}

	private void AddPlayerToLobby(long id) {
		var lobbyPlayerRes = GD.Load<PackedScene>("res://Scenes/UI/LobbyPlayer.tscn");
		var lobbyPlayer = lobbyPlayerRes.Instantiate();
		lobbyPlayer.Name = id.ToString();
		PlayerList.AddChild(lobbyPlayer);
	}

	private void RemovePlayerFromLobby(long id) {
		if (!PlayerList.HasNode(id.ToString()))
			return;

		PlayerList.GetNode(id.ToString()).QueueFree();
	}

	private void StartGame() {
		// Hide the UI and unpause to start the game.
		MainMenu.Hide();
		PlayerUi.Visible = true;
		// FIXME : Wtf is up with pause preventing the level from loading
		//GetTree().Paused = false;
		// Only change level on the server.
		// Clients will instantiate the level via the spawner.
		GD.Print("IsServer : " + Multiplayer.IsServer());
		if (Multiplayer.IsServer()) {
			GD.Print("Trying to load map");
			CallDeferred(MethodName.ChangeLevel, GD.Load<PackedScene>("res://Scenes/Levels/level.tscn"));
		}
	}

	// Call this function deferred and only on the main authority (server).
	private void ChangeLevel(PackedScene scene) {
		// Remove old level if any.
		foreach (Node c in Level.GetChildren()) {
			Level.RemoveChild(c);
			c.QueueFree();
		}
		// Add new level.
		Level.AddChild(scene.Instantiate());
		CurrentLevel.PlayerControls = PlayerControls;
	}
}
