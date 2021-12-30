extends KinematicBody

const GRAVITY: float = 9.8
const LINEAR_DAMP = 0.1
const ELEVATION_ANGLE = 45

var launched: bool = false

var previous_position: Vector3 = Vector3.ZERO
#var elapsed_time: float = 0
var velocity: Vector3

func _physics_process(delta: float) -> void:
	if launched:
		physics_step(delta)
#		if velocity.length() < 0.1 :
#			launched = false
			

func hit(direction : Vector2, elevated : bool, shot_power : float, spin : Vector2) -> void:
	if elevated:
		velocity = Vector3(direction.x,1,direction.y)*shot_power
	else:
		velocity = Vector3(direction.x, 0,direction.y)*shot_power
	launched = true

func physics_step(delta):
	#Adding gravity
	velocity.y += (velocity.y - GRAVITY) * delta
	
	var collision = move_and_collide(velocity/50, true, true, true)
	
	if collision:
		Global.debug["Collision"] = collision.normal
		Global.debug["Position"] = global_transform.origin
		#Linear dampening when on ground
		velocity.x = velocity.x - (velocity.x * LINEAR_DAMP * 0.1)
		velocity.z = velocity.z - (velocity.z * LINEAR_DAMP * 0.1)
		
		#Wall and slope handling
		velocity.x += collision.normal.x
		#FIX: Stop the ball from bouncing if it goes too fast in a slope
		velocity.y = max(-(velocity.y+collision.normal.y)/2, global_transform.origin.y - collision.position.y)
		velocity.z += collision.normal.z
	translate(velocity/50)
