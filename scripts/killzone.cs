using Godot;
using System;

public partial class killzone : Area2D
{
	private Timer timer;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player_mov player)
		{
			player.Die();
		}
		else if (body is Elephant elephant)
		{
			GD.Print("Elephant died ! T_T");
			elephant.Die();
		}
	}
}
