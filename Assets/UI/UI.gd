extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _input(event):
	if event.is_action_pressed("h"):
		win()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	$DebugLabel.text = ""
	for entry in Global.debug:
		$DebugLabel.text += str(entry) + " : " + str(Global.debug[entry]) + "\n"

func win():
	$UITween.interpolate_property($WinLabel, "rect_position:y", 600, 0, 2.5, Tween.TRANS_BOUNCE, Tween.EASE_OUT)
	$UITween.start()
