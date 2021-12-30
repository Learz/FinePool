extends Spatial

var player = 1
var _bounce_times = 5
var _reverse_direction = Vector2(-5,0)
var _spin = Vector2(0,0)
var _shot_power = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	$RigidBody.physics_material_override = PhysicsMaterial.new()
	$RigidBody.physics_material_override.friction = 1
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func hit(direction : Vector2, elevated : bool, shot_power : float, spin : Vector2):
	_spin = spin
	_shot_power = shot_power
	$RigidBody.physics_material_override.bounce = 0
	$RigidBody.linear_velocity = Vector3.ZERO
	$RigidBody.rotation = Vector3(0,0,0)
	if elevated:
		_bounce_times = 0
		_reverse_direction = direction.rotated(deg2rad(180))
		$RigidBody.physics_material_override.bounce = 1		
		$RigidBody.apply_impulse(Vector3(0,spin.y,0), Vector3(direction.x * shot_power,shot_power,direction.y * shot_power)/3)
	else:
		$RigidBody.apply_impulse(Vector3(0,0,0), Vector3(direction.x * shot_power, 0,direction.y * shot_power))

func _on_RigidBody_body_entered(body):
	_bounce_times += 1
	if _bounce_times > 5:
		$RigidBody.physics_material_override.bounce = 0
	if _bounce_times <= 5:
		$RigidBody.physics_material_override.bounce = 1/(_bounce_times*0.5)
#		$RigidBody.apply_impulse(Vector3(0,_spin.y/_bounce_times,0), Vector3(
##			_reverse_direction.x/(2*_bounce_times),
#			0,
#			_strength/(1.5*_bounce_times),
#			0))
##			_reverse_direction.y/(2*_bounce_times)))


func _on_RigidBody_sleeping_state_changed():
	if $RigidBody.sleeping:
		$RigidBody.transform.looking_at(Vector3(1,0,0), Vector3.UP)
