using Godot;
using System;

public partial class PlatformActivator : Node
{
	private Player_mov player;
	private Node2D stairway;
	
	public override void _Ready()
	{
		// Trouve le joueur quelque part dans la scène
		player = GetTree().Root.FindChild("Player", true, false) as Player_mov;

		// Récupère le groupe de plateformes
		stairway = GetNode<Node2D>("Stairway to Heaven");

		// Cache par défaut
		if (stairway != null)
			stairway.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (player == null || stairway == null)
			return;

		// Montre les plateformes si le joueur a l'éléphant
		stairway.Visible = player.HasElephant;
	}
}
