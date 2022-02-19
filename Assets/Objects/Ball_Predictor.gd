extends "res://Assets/Objects/Ball_v2.gd"

onready var mmi = get_node("../MultiMeshInstance")

var prediction_line:PoolVector3Array
var prediction_length = 200
var prediction_calculated = false

func _ready():
	_predictor = true

func _process(delta):
	#Move the trajectory predition spheres along the path
	if prediction_line.size() > 0:
		for i in mmi.multimesh.instance_count:
			mmi.multimesh.set_instance_transform(
				i, 
				Transform(
					Basis(),
					prediction_line[int((OS.get_ticks_msec()/60.0)+(i*10))%prediction_line.size()]
				)
			)

func _physics_process(delta: float) -> void:
	._physics_process(delta)
	if prediction_calculated == false:
		for i in prediction_length:
			$FloorCast.force_raycast_update()
			physics_step(delta)
			prediction_line.append(global_transform.origin)
		moving = false
		prediction_calculated = true

func set_shot(shot : ShotData):
	prediction_line.resize(0)
	transform.origin = Vector3.ZERO
	shot.power = shot.MAX_POWER
	hit(shot)
	prediction_length = 100 if shot.mode == shot.MODE.SHORT else 200

func calculate_prediction(shot : ShotData):
	set_shot(shot)
	prediction_calculated = false
	
