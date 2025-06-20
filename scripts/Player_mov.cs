using Godot;
using System;

public partial class Player_mov : CharacterBody2D
{
	private const float PIXELS_PER_UNIT = 1f;
	private const float MUL_RUN = 5f;
	private const float MUL_JUMP = 2f;

	// Movement
	public const float MAX_SPEED = MUL_RUN * 6f * 60f;
	public const float ACCEL = MUL_RUN * 7f * 60f;
	public const float DECEL = MUL_RUN * 50f * 60f;
	public const float TURN_SPEED = 80f * 60f;
	public const bool INSTANT_MOVEMENT = false;

	// Jumping
	public const float JUMP_HEIGHT = MUL_JUMP * 3.5f * 16f;
	public const float JUMP_DURATION = MUL_JUMP * 3.8f / 60f;
	public const float GRAVITY_DOWN = 3f * 60f * 60f;
	public const float JUMP_VELOCITY = -MUL_JUMP * (2f * JUMP_HEIGHT) / JUMP_DURATION;
	public const float JUMP_CUTOFF_MULTIPLIER = 0.6f;

	// Air control
	public const float AIR_ACCEL = MUL_RUN * 20f * 60f;
	public const float AIR_CONTROL = 40f;
	public const float AIR_BRAKE = 20f * 60f;

	// Assists
	public const float COYOTE_TIME_MAX = 0.2f;
	public const float JUMP_BUFFER_MAX = 0.2f;

	private float coyoteTimeCounter = 0f;
	private float jumpBufferCounter = 0f;
	private bool jumpHeld = false;

	// Wall jump
	public const float WALL_JUMP_X_VELOCITY = 200f;
	public const float WALL_JUMP_Y_VELOCITY = JUMP_VELOCITY;
	public const float WALL_JUMP_LOCK_DURATION = 0.2f;
	private bool isOnWall = false;
	private float wallJumpLockTimer = 0f;
	private int wallDirection = 0;

	// Gameplay
	public int n_coins = 0;
	public int n_deaths = 0;
	public bool EnableDropThrough = false;
	public bool HasElephant = false;
	[Export] public PackedScene ElephantScene;

	private Node2D elephantInstance = null;

	private AudioStreamPlayer2D footstepAudio;
	private AudioStreamPlayer2D jumpSound;
	private AudioStreamPlayer2D barrissementSound;
	private AnimatedSprite2D animatedSprite;

	public override void _Ready()
	{
		footstepAudio = GetNode<AudioStreamPlayer2D>("Footstepaudio");
		jumpSound = GetNode<AudioStreamPlayer2D>("JumpSound"); // ← ici
		barrissementSound = GetNode<AudioStreamPlayer2D>("BarrissementSound");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// COYOTE TIME
		if (IsOnFloor())
			coyoteTimeCounter = COYOTE_TIME_MAX;
		else
			coyoteTimeCounter -= dt;

		// JUMP BUFFER
		if (Input.IsActionJustPressed("ui_accept"))
			jumpBufferCounter = JUMP_BUFFER_MAX;
		else
			jumpBufferCounter -= dt;

		// VARIABLE HEIGHT
		if (!Input.IsActionPressed("ui_accept") && jumpHeld && velocity.Y < 0)
		{
			velocity.Y *= JUMP_CUTOFF_MULTIPLIER;
			jumpHeld = false;
		}

		// JUMP
		if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
			Jump(ref velocity);

		// GRAVITY
		if (!IsOnFloor())
		{
			if (velocity.Y < 0 && Input.IsActionPressed("ui_accept"))
				velocity.Y += (GRAVITY_DOWN * 0.5f) * dt;
			else
				velocity.Y += GRAVITY_DOWN * dt;
		}
		else
		{
			jumpHeld = false;
		}

		// MOVEMENT
		float input = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		float accel = IsOnFloor() ? ACCEL : AIR_ACCEL;
		float targetSpeed = input * MAX_SPEED;

		if (INSTANT_MOVEMENT)
		{
			velocity.X = targetSpeed;
		}
		else
		{
			if (input != 0)
			{
				float speedDiff = targetSpeed - velocity.X;
				float direction = Mathf.Sign(speedDiff);
				float appliedAccel = direction * (Mathf.Sign(velocity.X) != direction ? TURN_SPEED : accel) * dt;
				velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, Mathf.Abs(appliedAccel));
			}
			else
			{
				float decel = IsOnFloor() ? DECEL : AIR_BRAKE;
				velocity.X = Mathf.MoveToward(velocity.X, 0, decel * dt);
			}
		}

		// PLATFORM DROP THROUGH
		if (Input.IsActionJustPressed("ui_down") && IsOnFloor() && EnableDropThrough)
		{
			SetCollisionMaskValue(1, false);
			GetTree().CreateTimer(0.2f).Timeout += () =>
			{
				SetCollisionMaskValue(1, true);
			};
		}

		// FLIP SPRITE
		if (Mathf.Abs(velocity.X) > 5f)
			animatedSprite.FlipH = velocity.X < 0;

		// FOOTSTEP
		bool isMoving = Mathf.Abs(velocity.X) > 5f;
		bool onGround = IsOnFloor();
		if (isMoving && onGround)
		{
			if (!footstepAudio.Playing)
				footstepAudio.Play();
		}
		else
		{
			footstepAudio.Stop();
		}

		// WALL DETECTION
		isOnWall = IsOnWall() && !IsOnFloor();
		if (isOnWall)
			wallDirection = Velocity.X < 0 ? 1 : -1;

		// WALL JUMP
		if (jumpBufferCounter > 0f && isOnWall && !HasElephant)
		{
			Jump(ref velocity, WALL_JUMP_Y_VELOCITY);
			velocity.X = WALL_JUMP_X_VELOCITY * wallDirection;
			wallJumpLockTimer = WALL_JUMP_LOCK_DURATION;
		}

		// ELEPHANT DETACH
		if (HasElephant && Input.IsActionJustPressed("elephant_toggle"))
		{
			HasElephant = false;
			EnableDropThrough = false;

			if (ElephantScene != null)
			{
				Node2D elephant = ElephantScene.Instantiate<Node2D>();
				GetParent().AddChild(elephant);

				if (elephant is Elephant elephantScript)
					elephantScript.SetOwnerPlayer(this);

				Jump(ref velocity, JUMP_VELOCITY * 2f);

				if (!barrissementSound.Playing)
					barrissementSound.Play();

				Timer spawnTimer = new Timer();
				spawnTimer.WaitTime = 0.15;
				spawnTimer.OneShot = true;
				AddChild(spawnTimer);
				spawnTimer.Timeout += () => SpawnElephant();
				spawnTimer.Start();
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void Jump(ref Vector2 velocity, float customJumpVelocity = JUMP_VELOCITY)
	{
		velocity.Y = customJumpVelocity;
		coyoteTimeCounter = 0f;
		jumpBufferCounter = 0f;
		jumpHeld = true;

		if (!jumpSound.Playing)
			jumpSound.Play();
	}

	public void Die()
	{
		GD.Print("You died");
		Engine.TimeScale = 0.5;
		n_coins = 0;
		n_deaths++;
		GetNode<CollisionShape2D>("CollisionShape2D").QueueFree();
		GetNode<Timer>("Timer").Start(0.3);
	}

	private void SpawnElephant()
	{
		if (ElephantScene != null)
		{
			Node2D elephant = ElephantScene.Instantiate<Node2D>();
			GetParent().AddChild(elephant);

			if (elephant is Elephant elephantScript)
				elephantScript.SetOwnerPlayer(this);

			elephant.Position = GlobalPosition + new Vector2(0, 8);
		}
	}

	private void OnTimerTimeout()
	{
		Engine.TimeScale = 1.0;
		GetTree().ReloadCurrentScene();
	}
}
