using Godot;
using System;

public partial class Elephant : CharacterBody2D
{
	private AnimatedSprite2D sprite;
	private RayCast2D bottomLeft;
	private RayCast2D bottomRight;
	private RayCast2D upLeft;
	private RayCast2D upRight;

	private Vector2 velocity = Vector2.Zero;
	private const int Speed = 400; // Amélioration : vitesse un peu plus fluide
	private float gravity;

	private Player_mov ownerPlayer = null;
	private Vector2 startPosition;

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		bottomLeft = GetNode<RayCast2D>("LeftRaycast");
		bottomRight = GetNode<RayCast2D>("RightRaycast");
		upLeft = GetNode<RayCast2D>("UpLeftRaycast");
		upRight = GetNode<RayCast2D>("UpRightRaycast");

		gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
		startPosition = GlobalPosition;

		velocity.X = Speed;
		sprite.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// Gravité
		if (!IsOnFloor())
			velocity.Y += gravity * dt;
		else
			velocity.Y = 0;

		// Détection de bord : inverser direction si vide sous un des côtés
		if (!bottomRight.IsColliding() || upRight.IsColliding())
			velocity.X = -Mathf.Abs(Speed); // va à gauche
		else if (!bottomLeft.IsColliding() || upLeft.IsColliding())
			velocity.X = Mathf.Abs(Speed); // va à droite

		// Flip selon direction
		sprite.FlipH = velocity.X < 0;

		// Collision avec le joueur
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is Player_mov player && Input.IsActionJustPressed("elephant_toggle"))
			{
				player.HasElephant = true;
				player.EnableDropThrough = true;
				ownerPlayer = player;
				QueueFree(); // ou Hide() si tu veux le réutiliser
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Die()
	{
		if (ownerPlayer != null)
		{
			ownerPlayer.Die();
		}
		else
		{
			GD.PrintErr("Elephant has no owner — respawning to start position.");
			GlobalPosition = startPosition;
			velocity = Vector2.Zero;
		}
	}

	public void SetOwnerPlayer(Player_mov player)
	{
		ownerPlayer = player;
		GD.Print("Owner player set !");
	}
}
