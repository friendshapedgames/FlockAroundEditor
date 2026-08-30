extends PathFollow3D

@export var speed = 0.009

func _ready() -> void:
	progress_ratio = 0.5

func _process(delta):
	progress_ratio += speed * delta
	if progress_ratio >= 1.0:
		progress_ratio = 0.0
