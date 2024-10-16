using ShrimpleCharacterController;

namespace Lighthouse;

public sealed class PlayerController : Component
{
	[RequireComponent]
	public ShrimpleCharacterController.ShrimpleCharacterController Controller { get; set; }

	public SkinnedModelRenderer Renderer { get; set; }
	public CameraComponent Camera { get; set; }

	[Property]
	public Vector3 CameraOffset;

	[Property]
	[Range( 50f, 200f, 10f )]
	public float WalkSpeed { get; set; } = 100f;

	[Property]
	[Range( 100f, 500f, 20f )]
	public float RunSpeed { get; set; } = 300f;

	[Property]
	[Range( 25f, 100f, 5f )]
	public float DuckSpeed { get; set; } = 50f;

	[Property]
	[Range( 200f, 500f, 20f )]
	public float JumpStrength { get; set; } = 350f;

	public Angles EyeAngles { get; set; }
	private float _standingHeight = 64;
	private float _crouchingHeight = 32;

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants );

		Camera = Scene.GetAllComponents<CameraComponent>().First();
		if ( Camera is null )
		{
			Log.Error( "No camera found for PlayerController" );
		}

		Camera.ZFar = 32768f;
		Controller.ScaleAgainstWalls = false;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		var wishDirection = Input.AnalogMove.Normal * Rotation.FromYaw( EyeAngles.yaw );
		var isDucking = Input.Down( "Duck" );
		var isRunning = Input.Down( "Run" );
		var wishSpeed = isDucking ? DuckSpeed :
			isRunning ? RunSpeed : WalkSpeed;

		if ( isDucking )
		{
			CameraOffset.z = _crouchingHeight;
			Controller.TraceHeight = CameraOffset.z + 4;
		}
		else
		{
			CameraOffset.z = _standingHeight;
			Controller.TraceHeight = CameraOffset.z + 4;
		}

		Controller.WishVelocity = wishDirection * wishSpeed;
		Controller.Move();

		if ( Input.Pressed( "Jump" ) && Controller.IsOnGround )
		{
			Controller.Punch( Vector3.Up * JumpStrength );
			// AnimationHelper?.TriggerJump();
		}

		// if ( !AnimationHelper.IsValid() ) return;

		// AnimationHelper.WithWishVelocity( Controller.WishVelocity );
		// AnimationHelper.WithVelocity( Controller.Velocity );
		// AnimationHelper.DuckLevel = isDucking ? 1f : 0f;
		// AnimationHelper.IsGrounded = Controller.IsOnGround;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( MathX.Clamp( EyeAngles.pitch, -89, 89f ) );
		// Renderer.WorldRotation = Rotation.Slerp( Renderer.WorldRotation, Rotation.FromYaw( EyeAngles.yaw ), Time.Delta * 5f );

		Camera.WorldRotation = EyeAngles.ToRotation();
		Camera.LocalPosition = WorldPosition + CameraOffset;
	}
}
