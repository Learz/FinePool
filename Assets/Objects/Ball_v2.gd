extends KinematicBody

const GRAVITY = Vector3(0,-9.8,0)
const LINEAR_DAMP = 0.25
const ANGULAR_DAMP = 0.1
const ELEVATION_ANGLE = 45

var moving: bool = false

var previous_position: Vector3 = Vector3.ZERO
#var elapsed_time: float = 0
var velocity: Vector3
var last_velocities : PoolRealArray = [1,1,1,1,1]
var avg_vel = 0

var shot_data : ShotData = ShotData.new()

var spin : Vector2 = Vector2.ZERO
var angular_velocity: Vector3

var _predictor = false

func _physics_process(delta: float) -> void:
	if moving:
		physics_step(delta)
		Global.debug["Velocity before"] = velocity
		Global.debug["Position"] = global_transform.origin
		last_velocities.remove(0)
		last_velocities.append(Vector2(velocity.x,velocity.z).length())
		avg_vel = 0
		for vel in last_velocities:
			avg_vel += vel
		avg_vel = avg_vel / last_velocities.size()
		Global.debug["Avg Velocity"] = avg_vel
		if avg_vel < 0.2 and velocity.y <= 0.01 and velocity.y >= -0.01:
			moving = false
			

func hit(shot : ShotData) -> void:
	var direction = Vector3(1,0,0)
	if shot.mode == shot.MODE.HIGH:
		direction = direction.rotated(Vector3.BACK, deg2rad(60))
	direction = direction.rotated(Vector3.DOWN, shot.angle)
	velocity = Vector3(direction.x,direction.y,direction.z)*shot.power
	Global.debug["Shot_vector"] = direction
	shot_data = shot
	spin = Vector2(shot.spin.x/5, shot.spin.y)
	moving = true

func physics_step(delta):
	#Adding gravity
	velocity += GRAVITY * delta
	
	var collision = move_and_collide(velocity*delta, true, true, true)
	
	Global.debug["Collision normal"] = "None"
	Global.debug["Collision point"] = "None"
	
	if collision:
		if not _predictor:
			Global.debug["Collision normal"] = collision.normal
			Global.debug["Collision point"] = collision.position
		#Linear dampening when on ground
		
		#-----METHOD 1 BOUNCE IT WHEN NEEDED (Can't go uphill)-----
#		var bounce_vector = Vector3.ONE
#		if abs(collision.normal.x) > 0:
#			bounce_vector.x = -abs(collision.normal.x)
#		if abs(collision.normal.y) > 0:
#			bounce_vector.y = -abs(collision.normal.y)
#		if abs(collision.normal.z) > 0:
#			bounce_vector.z = -abs(collision.normal.z)
#		velocity = velocity*bounce_vector

		#-----METHOD 2 BOUNCE IT (Just plain bad, or not???)-----
		#FIX: The ball still vibrates up/down
		
#		if global_transform.origin.distance_to(collision.position) < 0.1:
#			velocity.y = collision.travel.y
		velocity = velocity.bounce(collision.normal)
		velocity.y /= 1.5
#		if abs(velocity.y) < 0.1:
#			velocity.y = stepify(velocity.y,0.01)

		#-----METHOD 3 - HIGH JANK MANUAL FIDDLE (uhhhhh) (still can't go up slopes)-----
#		#Wall and slope handling
#		if collision.normal.x == 1 or collision.normal.x == -1:
#			velocity.x = -velocity.x
#		else:
#			velocity.x += collision.normal.x
#		var normal_y = collision.normal.y
#		if int(round(normal_y)) == 1:
#			velocity.y = collision.position.y - global_transform.origin.y
#		else:
#			velocity.y += collision.normal.y
#		if collision.normal.z == 1 or collision.normal.z == -1:
#			velocity.z = -velocity.z
#		else:
#			velocity.z += collision.normal.z
	angular_velocity -= angular_velocity * ANGULAR_DAMP * 0.1
	if $FloorCast.is_colliding():
		velocity = velocity.rotated(Vector3.DOWN, spin.x * velocity.length() / 10)
		velocity.x = velocity.x - (velocity.x * LINEAR_DAMP * delta)
		velocity.z = velocity.z - (velocity.z * LINEAR_DAMP * delta)
#		spin.x = spin.x - (spin.x * LINEAR_DAMP * delta)
		angular_velocity = velocity
	global_translate(velocity*delta)
	$Ball.rotate_z(-angular_velocity.x*delta)
	$Ball.rotate_x(angular_velocity.z*delta)
	$Ball.global_rotate(Vector3.DOWN, spin.x * delta * 10)
