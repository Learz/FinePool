using System.Globalization;
using Godot;
using System;

public partial class Player : CharacterBody3D {


	public const float Force = 1.0f;
	public const float JumpVelocity = 4.5f;
	public const float ShotMaxPower = 15f;
	private const float LinearDamp = 0.99f;
	private const float AngularDamp = 0.1f;

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

	[Export] MeshInstance3D Ball;

	[Export] Sprite3D aimHint;

	public Level Level;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	Vector3 GravityVector {
		get {
			return new Vector3(0, -gravity, 0);
		}
	}

	private Vector3 AngularVelocity = new();

	private bool wasOnFloor = false;
	private bool JustTouchedFloor {
		get {
			return !wasOnFloor && IsOnFloor();
		}
	}
	private bool isFreeFloating = false;
	private bool wasFreeFloating = false;
	private Vector3 lastVelocity = Vector3.Zero;

	public override void _Ready() {
		if (IsMultiplayerAuthority()) {
			//PlayerInput.OnJump += Jump;
			PlayerInput.OnShoot += Shoot;
			PlayerInput.OnAimChanged += AimChanged;
		}

	}

	public override void _ExitTree() {
		if (IsMultiplayerAuthority()) {
			//PlayerInput.OnJump -= Jump;
			PlayerInput.OnShoot -= Shoot;
			PlayerInput.OnAimChanged -= AimChanged;
		}
	}

	public override void _PhysicsProcess(double delta) {
		Globals.Debug["Player " + playerId + " Is Turn"] = PlayerInput.IsTurn.ToString();
		if (!IsMultiplayerAuthority()) return;
		wasFreeFloating = GetSlideCollisionCount() <= 0;
		lastVelocity = Velocity;
		wasOnFloor = IsOnFloor();
		Vector3 velocity = Velocity;

		// Add the gravity.
		velocity.Y -= gravity * (float)delta;

		if (PlayerInput.CurrentControlMode == ControlMode.Move && IsOnFloor()) {
			// Get the input direction and handle the movement/deceleration.
			Vector3 direction = (Transform.Basis * new Vector3(PlayerInput.MoveDirection.X, 0, PlayerInput.MoveDirection.Y)).Normalized();
			if (direction != Vector3.Zero) {
				velocity.X += direction.X * Force;
				velocity.Z += direction.Z * Force;
			}
		}
		Velocity = velocity;
		if (IsOnFloor()) {
			Velocity *= LinearDamp;
			AngularVelocity = Velocity;
			Vector3 floorNormal = GetFloorNormal();
			Velocity = GetSlideVelocity(Velocity, floorNormal, delta);
		} else {
			AngularVelocity -= AngularVelocity * AngularDamp * 0.1f;
		}
		Ball.RotateZ(-AngularVelocity.X * ((float)delta));
		Ball.RotateX(AngularVelocity.Z * (float)delta);
		MoveAndSlide();
		isFreeFloating = GetSlideCollisionCount() <= 0;

		string collisionList = "";
		for (int i = 0; i < GetSlideCollisionCount(); i++) {
			var collision = GetSlideCollision(i);
			collisionList += ((Node)collision.GetCollider()).Name + ",";
			if (collision.GetCollider() is Player pl) {
				Velocity = lastVelocity.Bounce(collision.GetNormal()) / 2;
				pl.Velocity = lastVelocity / 2;
			}
			if (collision.GetNormal().Y <= float.Epsilon) {
				Velocity = lastVelocity.Bounce(collision.GetNormal()) * 0.95f;
			}
		}

		Globals.Debug["IsSliding"] = IsSliding().ToString();
		if (JustTouchedFloor) {
			if (lastVelocity.Y < -2f) {
				Velocity += GetLastSlideCollision().GetNormal() * (-lastVelocity.Y * 0.75f);
			}
		}
	}

	void Jump() {
		if (IsOnFloor()) {
			Velocity += Vector3.Up * JumpVelocity;
		}
	}

	void Shoot(double power) {
		if (IsOnFloor()) {
			Velocity += PlayerInput.Aim * (ShotMaxPower * (float)power);
		}
	}

	void AimChanged(Vector3 aim) {
		aimHint.Position = aim;
	}

	public bool IsSliding() {
		if (GetLastSlideCollision() == null) return false;
		// Calculate the dot product between the velocity and the surface normal
		float dotProduct = lastVelocity.Dot(GetLastSlideCollision().GetNormal());

		// Calculate the tangential component of the velocity
		Vector3 tangentialComponent = lastVelocity - GetLastSlideCollision().GetNormal() * dotProduct;

		// Check if the tangential component is non-zero
		return tangentialComponent.Length() > Mathf.Epsilon;
	}

	public Vector3 GetSlideVelocity(Vector3 velocity, Vector3 surfaceNormal, double delta) {
		// Normalize the surface normal just in case
		surfaceNormal = surfaceNormal.Normalized();

		// Calculate the component of gravity that is parallel to the slope
		Vector3 gravityAlongSlope = GravityVector - surfaceNormal * GravityVector.Dot(surfaceNormal);

		// Add the sliding effect to the current velocity
		Vector3 slideVelocity = velocity + gravityAlongSlope * (float)delta;

		return slideVelocity;
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ApplyForce(Vector3 force) {
		Velocity += force;
	}
}
