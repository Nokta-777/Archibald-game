using Godot;
using System;

public partial class Coin : Area2D
{
	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		// Connexion du signal
		this.Connect("body_entered", new Callable(this, nameof(_on_body_entered)));

		// Récupération du nœud AnimationPlayer
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public void _on_body_entered(Node body)
	{
		if (body is Player_mov player)
		{
			player.n_coins++;
			GD.Print($"+1 coin ! Total: {player.n_coins}");

			if (_animationPlayer != null)
				_animationPlayer.Play("pickup");
			else
				GD.PrintErr("AnimationPlayer non trouvé !");
		}
	}
}

