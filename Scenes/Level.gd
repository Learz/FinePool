extends Spatial

onready var ball = $Ball
onready var ball_predictor = $Ball/BallPredictor

#-----Shot Variables-----
var shot = ShotData.new()

#-----UI Variables-----
var _spin_modifier = false
var _power_select = false
var _power_reverse = false

var focus_cam = false

#-----Input Variables-----
const INIT_REPEATER_TIME = 4
const REG_REPEATER_TIME = 1
var _repeater_time = 0
var _start_repeat = false
var _repeat_action
var _repeatable_actions = [
	"ui_right",
	"ui_left"
]


# Called when the node enters the scene tree for the first time.
func _ready():
#	$Floor.make_baked_meshes()
#	$Floor.get_bake_meshes()
	pass


func _input(event):
	if event.is_action_pressed("focus_camera"):
		focus_cam = !focus_cam
	
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
			shot.spin.x += shot.SPIN_STEPS
			ball_predictor.calculate_prediction(shot)
		if event.is_action_pressed("ui_left"):
			shot.spin.x -= shot.SPIN_STEPS
			ball_predictor.calculate_prediction(shot)
		if event.is_action_pressed("ui_up"):
			shot.spin.y += shot.SPIN_STEPS
			ball_predictor.calculate_prediction(shot)
		if event.is_action_pressed("ui_down"):
			shot.spin.y -= shot.SPIN_STEPS
			ball_predictor.calculate_prediction(shot)
		$UI/Panel/Spin/SpinCursor.rect_position = Vector2(44,44) + Vector2(shot.spin.x, -shot.spin.y)*(5/shot.SPIN_STEPS)
	else:
		if event.is_action_pressed("ui_up"):
			shot.mode = clamp(shot.mode+1,0,2)
			ball_predictor.calculate_prediction(shot)
		if event.is_action_pressed("ui_down"):
			shot.mode = clamp(shot.mode-1,0,2)
			ball_predictor.calculate_prediction(shot)

	if event.is_action_pressed("ui_select"):
		if _power_select == true:
			_power_select = false
			ball.hit(shot)
		else:
			shot.power = 0
			_power_select = true
		


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	Global.debug["FPS"] = Engine.get_frames_per_second()
	_handle_repeatable_input(delta)
	_draw_UI(delta)
	
	#Move the the bal anchor to the ball position
	var ballPos = ball.global_transform.origin+Vector3.UP*0.5
	$BallAnchor.global_transform.origin = ballPos
	
	#FIX: Camera flickering while predicting (saved with lerp but still present)
	#Move the camera anchor to either the ball or the trajectory
	var cam_dest = Vector3.ZERO
	if ball.moving or focus_cam:
		cam_dest = Vector3(ballPos.x,clamp(ballPos.y, 0, 10),ballPos.z)
	else:
		cam_dest = (ball.global_transform.origin + ball_predictor.global_transform.origin)/2
	$CameraAnchor.global_transform.origin = lerp($CameraAnchor.global_transform.origin, cam_dest, delta*2)
	

func _physics_process(delta):
	pass


func _on_Sensor_area_entered(area):
	#Win condition (getting in the hole)
	if area.name == "Win":
		$BallAnchor/ShadowBillboard.visible = false
		$UI.win()
		print("You won bubba")


func _draw_UI(delta):
	#Display the shot power meter
	if _power_select == true:
		if shot.power >= shot.MAX_POWER:
			_power_reverse = true 
		if shot.power <= 0:
			_power_reverse = false 
		var mul = shot.MAX_POWER
		shot.power += -mul * delta if _power_reverse else mul * delta
		$UI/Panel/HitPowerBar.value = (shot.power / shot.MAX_POWER) * $UI/Panel/HitPowerBar.max_value


func _handle_repeatable_input(delta):
	if Input.is_action_pressed("ui_right") and (_repeater_time <= 0 or !_start_repeat):
		shot.angle += deg2rad(shot.ANGLE_STEPS)
		ball_predictor.calculate_prediction(shot)
	if Input.is_action_pressed("ui_left") and (_repeater_time <= 0 or !_start_repeat):
		shot.angle -= deg2rad(shot.ANGLE_STEPS)
		ball_predictor.calculate_prediction(shot)
	
	for action in _repeatable_actions:
		if Input.is_action_just_pressed(action):
			_repeater_time = INIT_REPEATER_TIME
			_start_repeat = true
		if Input.is_action_just_released(action):
			_start_repeat = false

	if _start_repeat:
		if _repeater_time <= 0:
			ball_predictor.calculate_prediction(shot)
			_repeater_time = REG_REPEATER_TIME
		_repeater_time -= 10*delta
