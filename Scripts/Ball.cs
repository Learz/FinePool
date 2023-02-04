using Godot;
using System;

public class Ball
{
    public const float LINEAR_DAMP = 0.25f;
    public const float ANGULAR_DAMP = 0.1f;
    public const float ELEVATION_ANGLE = 45f;

    public bool isMoving = false;
    public Vector3 previousPosition = Vector3.Zero;
    public Vector3 velocity = new Vector3();
    public Vector3[] velocityHistory;
    public float avgVelocity = 0f;

    public ShotData shotData = new ShotData();

    public Vector2 spin = Vector2.Zero;
    public Vector3 angularVelocity;
}
