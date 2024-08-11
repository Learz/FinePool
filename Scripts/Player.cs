using System.Globalization;
using Godot;
using System;

public partial class Player : CharacterBody3D {
	public PlayerInput PlayerInput {
		get { return (PlayerInput)GetNode("PlayerInput"); }
	}

	private int playerId = 1;

	[Export]
	public int PlayerId {
		get { return playerId; }
		set {
			playerId = value;
			PlayerInput.SetMultiplayerAuthority(playerId);
		}
	}

	[Export]
	public bool isTurn = false;

	[Export] Sprite3D aimHint;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float ShotMaxPower = 15f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready() {
		//PlayerInput.OnJump += Jump;
		PlayerInput.OnShoot += Shoot;
		PlayerInput.OnAimChanged += AimChanged;
	}

    public override void _ExitTree() {
		//PlayerInput.OnJump -= Jump;
		PlayerInput.OnShoot -= Shoot;
		PlayerInput.OnAimChanged -= AimChanged;
	}

    public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		if (PlayerInput.CurrentControlMode == ControlMode.None) {
			// Get the input direction and handle the movement/deceleration.
			Vector3 direction = (Transform.Basis * new Vector3(PlayerInput.MoveDirection.X, 0, PlayerInput.MoveDirection.Y)).Normalized();
			if (direction != Vector3.Zero) {
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			} else {
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}

		}
		Velocity = velocity;
		MoveAndSlide();
	}

	void Jump() {
		if (IsOnFloor()) {
			Velocity += Vector3.Up * JumpVelocity;
		}
	}

	void Shoot(double power) {
		if (IsOnFloor()) {
			Velocity += Vector3.Up * (ShotMaxPower * (float)power);
		}
	}

	void AimChanged(Vector3 aim) {
		GD.Print("Aim Changed to " + aim);
		aimHint.Position = aim;
	}
}
