using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using MoveControl2D;


public class PlayerMovement : MonoBehaviour
{
	// ===== runtime effect modifiers =====
	float _jumpHeightMul = 1f;      // násobí jumpHeight
	float _gravityMul = 1f;         // násobí sílu gravitace
	bool  _reverseGravity = false;   // obrácení směru
	float _glideMul = 1f;           // 0..1 (1 = normál, menší = vznášení)

	// veřejné settery pro efekty
	public void SetJumpHeightMultiplier(float mul) => _jumpHeightMul = Mathf.Max(0.01f, mul);
	public void ResetJumpHeightMultiplier() => _jumpHeightMul = 1f;

	public void SetReverseGravity(bool inv) => _reverseGravity = inv;
	public void ResetReverseGravity() => _reverseGravity = false;

	public void SetGravityMultiplier(float mul) => _gravityMul = Mathf.Max(0f, mul);
	public void ResetGravityMultiplier() => _gravityMul = 1f;

	public void SetGlideMultiplier(float mul) => _glideMul = Mathf.Clamp01(mul);
	public void ResetGlideMultiplier() => _glideMul = 1f;


	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	[SerializeField] Transform respawnPoint;
    Rigidbody2D _rb;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;

    private float _move = 0f;
    private bool _jumpPressed = false;
    private bool _downPressed = false;

    public void OnMove(InputAction.CallbackContext ctx) {
        _move = ctx.ReadValue<float>();
    }
    public void OnJump(InputAction.CallbackContext ctx) {
        if (ctx.started) _jumpPressed = true;
    }
    public void OnDrop(InputAction.CallbackContext ctx) {
        if (ctx.performed) _downPressed = true;
		if (ctx.canceled) _downPressed = false;
    }
    void LateUpdate() {
        _jumpPressed = false;
    }

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();
		_rb = GetComponent<Rigidbody2D>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if( _controller.isGrounded )
			_velocity.y = 0;

		if( _move != 0 )
		{
			normalizedHorizontalSpeed = _move;
			if (_move > 0) {
				if( transform.localScale.x < 0f )
					transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
			} 
			else if (_move < 0) {
				if( transform.localScale.x > 0f )
					transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
			}
			if( _controller.isGrounded )
				_animator.Play( Animator.StringToHash( "Run" ) );
		}
		else
		{
			normalizedHorizontalSpeed = 0;

			if( _controller.isGrounded )
				_animator.Play( Animator.StringToHash( "Idle" ) );
		}


		// effective gravity: tvůj gravity je záporný pro "dolů" :contentReference[oaicite:1]{index=1}
		float baseG = Mathf.Abs(gravity);
		float effectiveG = baseG * _gravityMul * (_reverseGravity ? +1f : -1f);

		// skok jen když jsi grounded
		if (_controller.isGrounded && _jumpPressed)
		{
			float effectiveJumpHeight = jumpHeight * _jumpHeightMul;
			float jumpSpeed = Mathf.Sqrt(2f * effectiveJumpHeight * Mathf.Abs(effectiveG));

			// skok je vždy proti gravitaci
			_velocity.y = -Mathf.Sign(effectiveG) * jumpSpeed;

			_animator.Play(Animator.StringToHash("Jump"));
		}

		// gravitační akcelerace (glide při pohybu ve směru gravitace)
		bool movingWithGravity = Mathf.Sign(_velocity.y) == Mathf.Sign(effectiveG);
		float gThisFrame = effectiveG * (movingWithGravity ? _glideMul : 1f);

		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

		// apply gravity before moving
		_velocity.y += gThisFrame * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets us jump down through one way platforms
		if( _controller.isGrounded && _downPressed )
		{
			// _velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
			if (_velocity.y > -1f) _velocity.y = -6f;
		}

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

	public void Respawn()
	{
		if (respawnPoint == null)
		{
			Debug.LogError("RespawnPoint není nastavený na PlayerMovement!");
			return;
		}

		// 1) Znič to, co hráč nese
		var carrier = GetComponent<PlayerCarrier>();
		if (carrier != null) carrier.DropAndDestroy();

		// 2) Reset runtime modifikátorů (ty máš už hotové settery)
		ResetJumpHeightMultiplier();
		ResetGravityMultiplier();
		ResetReverseGravity();
		ResetGlideMultiplier();
		var effectCtrl = GetComponent<PlayerEffectController>();
		if (effectCtrl != null)
			effectCtrl.ClearAllEffects();

		// 3) Vymaž efekty + GUI
		if (EffectManager.Instance != null)
			EffectManager.Instance.ClearAll();

		// 4) Teleport přes controller-safe postup (aby ti to controller nepřepsal)
		_velocity = Vector3.zero;
		normalizedHorizontalSpeed = 0f;

		if (_controller != null) _controller.enabled = false;
		transform.position = respawnPoint.position;
		if (_controller != null)
		{
			_controller.enabled = true;
			_controller.move(Vector3.zero);
		}

		if (_rb != null)
		{
			_rb.linearVelocity = Vector2.zero;
			_rb.angularVelocity = 0f;
		}
	}
}