using Godot;
using System;
using System.Linq;

public partial class PlayerControls : Control {


	[ExportGroup("Input")]
	private Player _player;
	[Export]
	Player Player {
		get {
			return _player;
		}
		set {
			if (_player != null) {
				_player.PlayerInput.OnPowerChanged -= OnPowerChanged;
			}
			_player = value;
			_player.PlayerInput.OnPowerChanged += OnPowerChanged;
			SpinGuide.PlayerInput = _player.PlayerInput;
			PlayerLabel.Text = "Player " + _player.PlayerId.ToString();
		}
	}

	[ExportGroup("Output")]
	[Export] PowerGauge PowerGauge;
	[Export] SpinGuide SpinGuide;
	[Export] Label PlayerLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ---- Listeners ----

	private void OnPowerChanged(double power) {
		PowerGauge.Value = power;
	}

	// ---- Functions ----
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AssignPlayerById(int playerId) {
		var foundPlayer = (Player)GetTree().GetNodesInGroup("players")?.Where(p => p.Name == playerId.ToString())?.First();
		if (foundPlayer == null) {
			GD.PrintErr("Could not find player ", playerId);
			return;
		}
		Player = foundPlayer;
		Player.PlayerInput.Reset();
	}
}
