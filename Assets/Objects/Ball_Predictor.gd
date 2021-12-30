extends "res://Assets/Objects/Ball_v2.gd"

func _physics_process(delta: float) -> void:
	if velocity.length() < 0.1 :
		launched = false
