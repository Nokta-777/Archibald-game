extends Control

func _on_texture_button_play_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/lvl1.tscn")


func _on_texture_button_options_pressed() -> void:
	print("WIP")


func _on_texture_button_quit_pressed() -> void:
	get_tree().quit(0)
