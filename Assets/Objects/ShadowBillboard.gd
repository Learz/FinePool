extends MeshInstance


onready var rays = {
	TR = get_node("RayCastTR"),
	TL = get_node("RayCastTL"),
	BR = get_node("RayCastBR"),
	BL = get_node("RayCastBL"),
}

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	visible = true
	var colliding_counter = 0
	for ray in rays.values():
		if ray.is_colliding():
			colliding_counter += 1
#		ray.look_at(ray.global_transform.origin + Vector3.DOWN, Vector3.UP)
#		ray.global_transform.origin = ray.get_collision_point() + Vector3(0,2,0)
	if colliding_counter <= 2:
		visible = false
	var height_min = max(rays.TL.get_collision_point().y, max(rays.TR.get_collision_point().y, max(rays.BL.get_collision_point().y, rays.BR.get_collision_point().y))) 
	global_transform.origin.y = 0.01 + height_min
	#TODO: Working shadow size based on length
	var size = lerp(1,0,(rays.TL.get_collision_point().y-rays.TL.global_transform.origin.y)/10)
	scale = Vector3(size,size,1)
#	var normal_avg = (rays.TL.get_collision_normal() + rays.TR.get_collision_normal() + rays.BL.get_collision_normal() + rays.BR.get_collision_normal()) / 4
#	look_at(global_transform.origin + normal_avg, Vector3.RIGHT)
