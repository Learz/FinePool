using Godot;
using System;

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
	public void AssignPlayer(Player p) {
		if (p == null) {
			Player = (Player)GetTree().GetFirstNodeInGroup("players");
			GD.Print("No Player Assigned, defaulting to " + Player);
		} else if (Player != p) {
			Player = p;
		}
		Player.PlayerInput.Reset();
	}
}
