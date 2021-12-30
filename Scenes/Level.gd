extends Spatial

#-----Shot Variables-----
enum SHOOT_MODE {LONG, SHORT, HIGH}
var shoot_mode = SHOOT_MODE.SHORT

var angle = 0
const ANGLE_STEPS = 5

var shot_power = 0.5
const MAX_SHOT_POWER = 5

var direction = Vector2(1,0)

var spin = Vector2(0,0)
const SPIN_STEPS = 0.1

var prediction_line:PoolVector3Array
var prediction_length = 200
var prediction_calculated = false

#-----UI Variables-----
var _spin_modifier = false
var _power_select = false
var _power_reverse = false

#-----Input Variables-----
const INIT_REPEATER_TIME = 4
const REG_REPEATER_TIME = 2
var _repeater_time = 0
var _start_repeat = false
var _repeat_action
var _repeatable_actions = [
	"ui_right",
	"ui_left"
]


# Called when the node enters the scene tree for the first time.
func _ready():
	$Floor.make_baked_meshes()
	$Floor.get_bake_meshes()
	pass


func _input(event):
	if event.is_action_pressed("ui_reset"):
		get_tree().reload_current_scene()
		
	if event.is_action_pressed("ui_spin_modifier"):
		$UI/Panel/Spin/SpinCursor.self_modulate = Color(0,0.5,0)
		_spin_modifier = true
	if event.is_action_released("ui_spin_modifier"):
		_spin_modifier = false
		$UI/Panel/Spin/SpinCursor.self_modulate = Color(0.5,0,0)
	
	if _spin_modifier:
		if event.is_action_pressed("ui_right"):
			prediction_calculated = false
			spin.x += SPIN_STEPS
		if event.is_action_pressed("ui_left"):
			prediction_calculated = false
			spin.x -= SPIN_STEPS
		if event.is_action_pressed("ui_up"):
			prediction_calculated = false
			spin.y += SPIN_STEPS
		if event.is_action_pressed("ui_down"):
			prediction_calculated = false
			spin.y -= SPIN_STEPS
		$UI/Panel/Spin/SpinCursor.rect_position = Vector2(44,44) + Vector2(spin.x, -spin.y)*(5/SPIN_STEPS)
	else:
		if event.is_action_pressed("ui_up"):
			prediction_calculated = false
			shoot_mode = clamp(shoot_mode+1,0,2)
		if event.is_action_pressed("ui_down"):
			prediction_calculated = false
			shoot_mode = clamp(shoot_mode-1,0,2)

	if event.is_action_pressed("ui_select"):
		if _power_select == true:
			_power_select = false
			$Ball.hit(direction, true if shoot_mode == SHOOT_MODE.HIGH else false, shot_power, spin)
		else:
			shot_power = 0
			_power_select = true
		


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	Global.debug["FPS"] = Engine.get_frames_per_second()
	_handle_repeatable_input(delta)
	_draw_UI(delta)
	
	#Move the the bal anchor to the ball position
	var ballPos = $Ball.global_transform.origin+Vector3.UP*0.5
	$BallAnchor.global_transform.origin = ballPos
	
	#Move the camera anchor to either the ball or the trajectory
#	if $Ball.launched:
	$CameraAnchor.global_transform.origin = Vector3(ballPos.x,clamp(ballPos.y, 0, 10),ballPos.z)
#	else:
#		$CameraAnchor.global_transform.origin = ($Ball.global_transform.origin + $BallPredictor.global_transform.origin)/2
	
	#Move the trajectory predition spheres along the path
	for i in $MultiMeshInstance.multimesh.instance_count:
		$MultiMeshInstance.multimesh.set_instance_transform(
			i, 
			Transform(
				Basis(),
				prediction_line[int((OS.get_ticks_msec()/60.0)+(i*10))%prediction_length]
			)
		)


func _physics_process(delta):
	#Ball trajectory prediction
	if prediction_calculated == false:
		prediction_line.resize(0)
		$BallPredictor.global_transform.origin = $Ball.global_transform.origin
		$BallPredictor.hit(
			direction, 
			true if shoot_mode == SHOOT_MODE.HIGH else false, 
			MAX_SHOT_POWER, 
			spin)
		prediction_length = 100 if shoot_mode == SHOOT_MODE.SHORT else 200
		for i in prediction_length:
			$BallPredictor.physics_step(delta)
			prediction_line.append($BallPredictor.global_transform.origin)
		$BallPredictor.launched = false
		prediction_calculated = true


func _on_Sensor_area_entered(area):
	#Win condition (getting in the hole)
	if area.name == "Win":
		$BallAnchor/ShadowBillboard.visible = false
		$UI.win()
		print("You won bubba")


func _draw_UI(delta):
	#Display the shot power meter
	if _power_select == true:
		if shot_power >= MAX_SHOT_POWER:
			_power_reverse = true 
		if shot_power <= 0:
			_power_reverse = false 
		var mul = MAX_SHOT_POWER
		shot_power += -mul * delta if _power_reverse else mul * delta
		$UI/Panel/HitPowerBar.value = (shot_power / MAX_SHOT_POWER) * $UI/Panel/HitPowerBar.max_value


func _handle_repeatable_input(delta):
	if Input.is_action_pressed("ui_right") and (_repeater_time <= 0 or !_start_repeat):
		prediction_calculated = false
		angle += deg2rad(ANGLE_STEPS)
		direction = Vector2(1,0).rotated(angle)
	if Input.is_action_pressed("ui_left") and (_repeater_time <= 0 or !_start_repeat):
		prediction_calculated = false
		angle -= deg2rad(ANGLE_STEPS)
		direction = Vector2(1,0).rotated(angle)
	
	for action in _repeatable_actions:
		if Input.is_action_just_pressed(action):
			_repeater_time = INIT_REPEATER_TIME
			_start_repeat = true
		if Input.is_action_just_released(action):
			_start_repeat = false

	if _start_repeat:
		if _repeater_time <= 0:
			prediction_calculated = false
			_repeater_time = REG_REPEATER_TIME
		_repeater_time -= 10*delta
