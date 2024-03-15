using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "7eb5cbfb6c98181a790c65d41fb95f354d95dfc7")]
public class EnemyFireController : Component
{
	public Node leftMuzzle = null;
	public Node rightMuzzle = null;

	public AssetLink bulletPrefab = null;

	public float shootInterval = 1.0f;

	private float currentTime = 0.0f;
	private bool isLeft = false;
	private bool isFiring = false;

	public void StartFiring()
	{
		isFiring = true;
	}

	public void StopFiring()
	{
		isFiring = false;
	}

	private void Init()
	{
		// сброс таймера
		currentTime = 0.0f;
		// переключаем стрельбу на правый ствол
		isLeft = false;
	}

	private void Update()
	{
		// если робот не в состоянии атаки (Бездействие или Преследование), то ничего не делаем
		if (!isFiring)
			return;

		// обновляем таймер
		currentTime += Game.IFps;

		// проверка интервала стрельбы
		if (currentTime > shootInterval)
		{
			// сброс таймера
			currentTime -= shootInterval;
			// создаем пулю из ассета назначенного в bulletPrefab
			Node bullet = World.LoadNode(bulletPrefab.AbsolutePath);

			// устанавливаем положение пули в зависимости от того, с какой стороны стреляем
			bullet.WorldTransform = (isLeft) ? leftMuzzle.WorldTransform : rightMuzzle.WorldTransform;
			// меняем ствол для следующего выстрела
			isLeft = !isLeft;

		}
	}
}