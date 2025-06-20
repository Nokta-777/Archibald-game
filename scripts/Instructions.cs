using Godot;
using System;

public partial class Instructions : Label
{
	private Player_mov player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetTree().Root.FindChild("Player", true, false) as Player_mov;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!player.HasElephant)
		{
			Text = "→ to run to the right\n← to run to the left\nSPACE to jump\n\nBring back the elephant to the beginning of the level";
		} else {
			Text = "Left to exit";
		}
	}
}
