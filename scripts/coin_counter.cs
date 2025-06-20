using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class coin_counter : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var player = GetNode<Player_mov>("../../Player"); // adapte le chemin
		int coins = player.n_coins;
		this.Text = $"Coins : {coins}";
	}
}
