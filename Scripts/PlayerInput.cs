using Godot;
using System;

public enum ControlMode {
	None,
	Aim,
	Spin,
	Shoot
}

public partial class PlayerInput : MultiplayerSynchronizer {
	[Signal]
	public delegate void OnJumpEventHandler();
	[Signal]
	public delegate void OnShootEventHandler(double power);

	[Export]
	public Vector2 MoveDirection = new();
	[Export]


	private Vector3 _aim = Vector3.Forward;
	[Signal]
	public delegate void OnAimChangedEventHandler(Vector3 aim);
	public Vector3 Aim {
		get { return _aim; }
		set {
			_aim = value;
			EmitSignal(SignalName.OnAimChanged, _aim);
		}
	}

	private double fineGrainedPower = 0.0;
	[Export(PropertyHint.Range, "0,1,0.1")]

	private double _power;
	[Signal]
	public delegate void OnPowerChangedEventHandler(double power);
	public double Power {
		get { return _power; }
		set {
			_power = Math.Round(value * 10) / 10;
			EmitSignal(SignalName.OnPowerChanged, Power);
		}
	}
	private double powerDirection = 1;

	private Vector2 _spin;

	[Signal]
	public delegate void OnSpinChangedEventHandler(Vector2 spin);
	[Export]
	public Vector2 Spin {
		get { return _spin; }
		set {
			_spin = ClampWithinCircle(value);
			EmitSignal(SignalName.OnSpinChanged, _spin);
		}
	}

	public ControlMode CurrentControlMode = ControlMode.None;

	// ---- Overrides ----
	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_select")) {
			fineGrainedPower = 0;
			CurrentControlMode = CurrentControlMode != ControlMode.Shoot ? ControlMode.Shoot : ControlMode.None;
		}
		if (@event.IsActionPressed("dev_m")) {
			CurrentControlMode = CurrentControlMode != ControlMode.None ? ControlMode.None : ControlMode.Aim;
		}

		switch (CurrentControlMode) {
			case ControlMode.Spin:
				if (@event.IsActionPressed("ui_up", true)) Spin += Vector2.Up / 10;
				if (@event.IsActionPressed("ui_down", true)) Spin += Vector2.Down / 10;
				if (@event.IsActionPressed("ui_left", true)) Spin += Vector2.Left / 10;
				if (@event.IsActionPressed("ui_right", true)) Spin += Vector2.Right / 10;
				break;
			case ControlMode.Aim:
				if (@event.IsActionPressed("ui_left", true)) Aim = Aim.Rotated(Vector3.Up, Mathf.DegToRad(15));
				if (@event.IsActionPressed("ui_right", true)) Aim = Aim.Rotated(Vector3.Up, Mathf.DegToRad(-15));
				break;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SetProcess(GetMultiplayerAuthority() == Multiplayer.GetUniqueId());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		handleMode(delta);

		if (CurrentControlMode == ControlMode.None) {
			handleFreeMove();
		}
	}

	// ---- Functions ----

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Jump() {
		EmitSignal(SignalName.OnJump);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Shoot() {
		EmitSignal(SignalName.OnShoot, Power);
	}

	private void handleFreeMove() {
		MoveDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (Input.IsActionJustPressed("ui_accept")) {
			Rpc(nameof(Shoot));
		}
	}

	private void handleMode(double delta) {
		if (Input.IsActionPressed("aim_mode")) {
			CurrentControlMode = ControlMode.Spin;
		}
		if (Input.IsActionJustReleased("aim_mode")) {
			CurrentControlMode = ControlMode.None;
		}
		if (CurrentControlMode == ControlMode.Shoot) {
			handlePower(delta);
		}
	}

	private void handlePower(double delta) {
		if (fineGrainedPower >= 1.1) powerDirection = -1;
		if (fineGrainedPower <= -0.1) powerDirection = 1;
		fineGrainedPower += delta * powerDirection;
		Power = fineGrainedPower;
	}

	Vector2 ClampWithinCircle(Vector2 vector) {
		// Step 1: Translate the small circle's position relative to the large circle's center
		Vector2 clampedVector = vector;

		// Step 2: Calculate the maximum allowed distance from the large circle's center
		float maxDistance = 1;

		// Step 3: Calculate the length of the relative position vector
		float length = clampedVector.Length();

		// Step 4: Clamp the length to the maximum distance
		if (length > maxDistance) {
			clampedVector = clampedVector / length * maxDistance;
		}

		Vector2 normalizedVector = clampedVector / maxDistance;
		return normalizedVector;
	}

	public void Reset() {
		Aim = Vector3.Forward;
		fineGrainedPower = 0.0;
		Power = 0.0;
		Spin = Vector2.Zero;
		MoveDirection = Vector2.Zero;
	}
}
