using Godot;
using System;

public class ShotData
{
    public enum Mode {LONG, SHORT, HIGH};
	public Mode mode = Mode.SHORT;

	public float angle = 0f;
	const float ANGLE_STEPS = 2f;

	public float power = 0.5f;
	const float MAX_POWER = 10f;

	//var direction = Vector2(1,0);

	public Vector2 spin = new Vector2(0,0);
	const float SPIN_STEPS = 0.1f;
}
