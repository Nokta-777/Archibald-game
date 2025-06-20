using Godot;
using System;

public partial class PlatformBlue : StaticBody2D
{
	[Export] public float DisappearDelay = 0.5f;
	[Export] public float ReappearDelay = 2.0f;

	private Sprite2D sprite;
	private CollisionShape2D collision;

	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
		collision = GetNode<CollisionShape2D>("CollisionShape2D");

		var area = GetNode<Area2D>("Area2D");
		area.BodyEntered += OnBodyEntered;
	}

	private async void OnBodyEntered(Node body)
	{
		if (body is Player_mov)
		{
			await ToSignal(GetTree().CreateTimer(DisappearDelay), "timeout");
			SetActive(false);

			await ToSignal(GetTree().CreateTimer(ReappearDelay), "timeout");
			SetActive(true);
		}
	}

	private void SetActive(bool active)
	{
		sprite.Visible = active;
		collision.Disabled = !active;
	}
}
