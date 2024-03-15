using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "96c240040ff7888aa3804c36b43a466e1b430b9e")]
public class PlayerLogic : Component
{
	private Health health = null;
	private GameController gameController = null;

	private void Init()
	{
		// берем у ноды компонент Health
		health = node.GetComponentInChildren<Health>();
		// обновляем информацию об исходном здоровье игрока
		ComponentSystem.FindComponentInWorld<HUD>().UpdateHealthInfo(health.health);

		// находим компонент GameController
		gameController = ComponentSystem.FindComponentInWorld<GameController>();
	}

	private void Update()
	{
		if (health && health.IsDead)
		{
			// обездвиживаем игрока, отключая компоненты
			node.GetComponent<FirstPersonController>().Enabled = false;
			node.GetComponent<WeaponController>().Enabled = false;
			node.GetComponent<ShootInput>().Enabled = false;

			// удаляем игрока
			node.DeleteLater();

			// меняем состояние игрового процесса на (Lose - поражение)
			gameController.state = GameState.Lose;
		}
		// проверяем состояние игры, если она окончена, удаляем игрока
		else if (gameController.state != GameState.Gameplay)
			node.DeleteLater();
	}
}