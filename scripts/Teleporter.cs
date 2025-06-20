using Godot;
using System;

public partial class Teleporter : Area2D
{
	[Export(PropertyHint.File, "*.tscn")] private string GoToScenePath;

	public override void _Ready()
	{
	}

	public override void _PhysicsProcess(double delta)
	{
	}

	private void _on_body_entered(Node2D body)
	{
		var currentScene = GetTree().CurrentScene;
		if (body is Player_mov player && player.HasElephant)
		{
			CallDeferred(nameof(DeferredChangeScene));
		}
	}

	private void DeferredChangeScene()
	{
		GetTree().ChangeSceneToFile(GoToScenePath);
	}

}
