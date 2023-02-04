using Godot;
using System;
using ExtensionMethods;


public class BallController : KinematicBody
{
    Ball ball = new Ball();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready(){
        ball.velocity.x = 5f;
        this.GetGlobal().debug["Velocity before"] = ball.velocity;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta){
        //if (ball.isMoving){
            physicsStep(delta);
        //}
    }

    private void physicsStep(float delta){
        ball.velocity += this.GetGlobal().GRAVITY * delta;

        KinematicCollision collision = MoveAndCollide(ball.velocity * delta, true, true, true);

        this.GetGlobal().debug["Collision normal"] = "None";
        this.GetGlobal().debug["Collision point"] = "None";

        if (collision != null){
            if(!(ball is BallPredictor)){
                this.GetGlobal().debug["Collision normal"] = collision.Normal;
                this.GetGlobal().debug["Collision point"] = collision.Position;
            }

            ball.velocity = ball.velocity.Bounce(collision.Normal);
		    ball.velocity.y /= 1.5f;
        }
        ball.angularVelocity -= ball.angularVelocity * Ball.ANGULAR_DAMP * 0.1f;
        if (GetNode<RayCast>("FloorCast").IsColliding()){
            ball.velocity = ball.velocity.Rotated(Vector3.Down, ball.spin.x * ball.velocity.Length() / 10);
            ball.velocity.x = ball.velocity.x - (ball.velocity.x * Ball.LINEAR_DAMP * delta);
            ball.velocity.z = ball.velocity.z - (ball.velocity.z * Ball.LINEAR_DAMP * delta);
            ball.angularVelocity = ball.velocity;
        }

        GlobalTranslate(ball.velocity * delta);
        GetNode<MeshInstance>("Ball").RotateX(-ball.angularVelocity.z*delta);
        GetNode<MeshInstance>("Ball").RotateZ(-ball.angularVelocity.x*delta);
        GetNode<MeshInstance>("Ball").GlobalRotate(Vector3.Down, ball.spin.x * delta * 10);

    }
}
