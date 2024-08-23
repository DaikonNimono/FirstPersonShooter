using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "f21127b33f4bf8120d79170e808b628f1d16ea1a")]
public class HandAnimationController : Component
{
	// контроллер игрока с видом от первого лица (FirstPersonController)
	public FirstPersonController fpsController = null;

	public ShootInput shootInput = null;

	public float moveAnimationSpeed = 30.0f;
	public float shootAnimationSpeed = 30.0f;
	public float idleWalkMixDamping = 5.0f;
	public float walkDamping = 5.0f;
	public float shootDamping = 1.0f;

	// параметры анимации
	[ParameterFile(Filter = ".anim")]
	public string idleAnimation = null;

	[ParameterFile(Filter = ".anim")]
	public string moveForwardAnimation = null;

	[ParameterFile(Filter = ".anim")]
	public string moveBackwardAnimation = null;

	[ParameterFile(Filter = ".anim")]
	public string moveRightAnimation = null;

	[ParameterFile(Filter = ".anim")]
	public string moveLeftAnimation = null;

	[ParameterFile(Filter = ".anim")]
	public string shootAnimation = null;

	public vec2 LocalMovementVector
	{
		get
		{
			return new vec2(
				MathLib.Dot(fpsController.SlopeAxisY, fpsController.HorizontalVelocity),
				MathLib.Dot(fpsController.SlopeAxisX, fpsController.HorizontalVelocity)
			);
		}
		set { }
	}

	private ObjectMeshSkinned meshSkinned = null;
	private float currentIdleWalkMix = 0.0f; // 0 анимация покоя, 1 анимация ходьбы
	private float currentShootMix = 0.0f; // 0 комбинация бездействие/ходьба, 1 анимация стрельбы
	private float currentWalkForward = 0.0f;
	private float currentWalkBackward = 0.0f;
	private float currentWalkRight = 0.0f;
	private float currentWalkLeft = 0.0f;

	private float currentWalkIdleMixFrame = 0.0f;
	private float currentShootFrame = 0.0f;
	private int numShootAnimationFrames = 0;

	// задаем число анимационных слоев
	private const int numLayers = 6;

	private void Init()
	{
		// берем ноду, которой назначена компонента и 
		// и преобразовываем ее к типу ObjectMeshSkinned
		meshSkinned = node as ObjectMeshSkinned;

		// устанавливаем количество анимационных слоев для каждого объекта
		meshSkinned.NumLayers = numLayers;

		// set animation for each animation layer
		meshSkinned.SetLayerAnimationFilePath(0, idleAnimation);
		meshSkinned.SetLayerAnimationFilePath(1, moveForwardAnimation);
		meshSkinned.SetLayerAnimationFilePath(2, moveBackwardAnimation);
		meshSkinned.SetLayerAnimationFilePath(3, moveRightAnimation);
		meshSkinned.SetLayerAnimationFilePath(4, moveLeftAnimation);
		meshSkinned.SetLayerAnimationFilePath(5, shootAnimation);

		numShootAnimationFrames = meshSkinned.GetLayerNumFrames(5);

		// включаем все анимационные слои
		for (int i = 0; i < numLayers; ++i)
			meshSkinned.SetLayerEnabled(i, true);
	}

	public void Shoot()
	{
		// включаем анимацию стрельбы
		currentShootMix = 1.0f;
		// устанавливаем кадр анимационного слоя в 0
		currentShootFrame = 0.0f;
	}

	private void Update()
	{
		vec2 movementVector = LocalMovementVector;

		// проверяем, движется ли персонаж
		bool isMoving = movementVector.Length2 > MathLib.EPSILON;

		// handle input: check if the fire button is pressed
		if (shootInput.IsShooting())
			Shoot();  
		// calculate the target values for the layer weights
		float targetIdleWalkMix = (isMoving) ? 1.0f : 0.0f;
		float targetWalkForward = (float) MathLib.Max(0.0f, movementVector.x);
		float targetWalkBackward = (float) MathLib.Max(0.0f, -movementVector.x);
		float targetWalkRight = (float) MathLib.Max(0.0f, movementVector.y);
		float targetWalkLeft = (float) MathLib.Max(0.0f, -movementVector.y);

		// применяем текущие весовые коэффициенты
		float idleWeight = 1.0f - currentIdleWalkMix;
		float walkMixWeight = currentIdleWalkMix;
		float shootWalkIdleMix = 1.0f - currentShootMix;

		meshSkinned.SetLayerWeight(0, shootWalkIdleMix * idleWeight);
		meshSkinned.SetLayerWeight(1, shootWalkIdleMix * walkMixWeight * currentWalkForward);
		meshSkinned.SetLayerWeight(2, shootWalkIdleMix * walkMixWeight * currentWalkBackward);
		meshSkinned.SetLayerWeight(3, shootWalkIdleMix * walkMixWeight * currentWalkRight);
		meshSkinned.SetLayerWeight(4, shootWalkIdleMix * walkMixWeight * currentWalkLeft);
		meshSkinned.SetLayerWeight(5, currentShootMix);

		// update the animation frames: set the same frame for the animation layers to keep them in sync
		meshSkinned.SetLayerFrame(0, currentWalkIdleMixFrame);
		meshSkinned.SetLayerFrame(1, currentWalkIdleMixFrame);
		meshSkinned.SetLayerFrame(2, currentWalkIdleMixFrame);
		meshSkinned.SetLayerFrame(3, currentWalkIdleMixFrame);
		meshSkinned.SetLayerFrame(4, currentWalkIdleMixFrame);
		// set the shooting animation layer frame to 0 to start animation from the beginning
		meshSkinned.SetLayerFrame(5, currentShootFrame);

		currentWalkIdleMixFrame += moveAnimationSpeed * Game.IFps;
		currentShootFrame = MathLib.Min(currentShootFrame + shootAnimationSpeed * Game.IFps, numShootAnimationFrames);

		// smoothly update the current weight values
		currentIdleWalkMix = MathLib.Lerp(currentIdleWalkMix, targetIdleWalkMix, idleWalkMixDamping * Game.IFps);

		currentWalkForward = MathLib.Lerp(currentWalkForward, targetWalkForward, walkDamping * Game.IFps);
		currentWalkBackward = MathLib.Lerp(currentWalkBackward, targetWalkBackward, walkDamping * Game.IFps);
		currentWalkRight = MathLib.Lerp(currentWalkRight, targetWalkRight, walkDamping * Game.IFps);
		currentWalkLeft = MathLib.Lerp(currentWalkLeft, targetWalkLeft, walkDamping * Game.IFps);

		currentShootMix = MathLib.Lerp(currentShootMix, 0.0f, shootDamping * Game.IFps);
	}
}