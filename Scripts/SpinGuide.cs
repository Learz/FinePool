using Godot;

public partial class SpinGuide : TextureRect {
	[Export] TextureRect AimDot;
	[Export] Label AimInfo;

	private PlayerInput _playerInput;
	[Export]
	public PlayerInput PlayerInput {
		get {
			return _playerInput;
		}
		set {
			if (_playerInput != null) {
				_playerInput.OnSpinChanged -= AimChanged;
			}
			_playerInput = value;
			_playerInput.OnSpinChanged += AimChanged;
		}
	}

	public Vector2 SteppedAim {
		get {
			if (PlayerInput == null) {
				return Vector2.Zero;
			}
			return (PlayerInput.Spin * 10).Round() / 10;
		}
	}
	bool pressing = false;
	bool isMouseOver = false;

	public override void _Ready() {
		MouseEntered += () => { isMouseOver = true; };
		MouseExited += () => { isMouseOver = false; };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (PlayerInput == null) return;
		if ((Input.IsActionJustPressed("left_click") && isMouseOver) || (Input.IsMouseButtonPressed(MouseButton.Left) && pressing)) {
			pressing = true;
			var mousePos = GetViewport().GetMousePosition();
			PlayerInput.Spin = ClampCircleWithinCircle(mousePos, AimDot.Size, GlobalPosition, Size);
		} else {
			pressing = false;
		}


	}

	void AimChanged(Vector2 newAim) {
		AimDot.GlobalPosition = GlobalPosition + (Size / 2) + (SteppedAim * ((Size / 2) - (AimDot.Size / 2))) - (AimDot.Size / 2);
		AimInfo.Text = SteppedAim.ToString();
	}

	Vector2 ClampCircleWithinCircle(Vector2 smallCirclePos, Vector2 smallCircleSize, Vector2 largeCirclePos, Vector2 largeCircleSize) {
		Vector2 largeCircleCenter = largeCirclePos + (largeCircleSize / 2);

		// Step 1: Translate the small circle's position relative to the large circle's center
		Vector2 relativePos = smallCirclePos - largeCircleCenter;

		// Step 2: Calculate the maximum allowed distance from the large circle's center
		float maxDistance = (largeCircleSize.X / 2) - (smallCircleSize.X / 2);

		// Step 3: Calculate the length of the relative position vector
		float length = relativePos.Length();

		// Step 4: Clamp the length to the maximum distance
		if (length > maxDistance) {
			relativePos = relativePos / length * maxDistance;
		}

		Vector2 normalizedVector = relativePos / maxDistance;
		return normalizedVector;
	}
}
