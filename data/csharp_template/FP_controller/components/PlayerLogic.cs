using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "96c240040ff7888aa3804c36b43a466e1b430b9e")]
public class PlayerLogic : Component
{
	private Health health = null;
	private void Init()
	{
		// берем у ноды компонент Health
		health = node.GetComponentInChildren<Health>();
		// обновляем информацию об исходном здоровье игрока
		ComponentSystem.FindComponentInWorld<HUD>().UpdateHealthInfo(health.health);
	}

	private void Update()
	{
		// проверяем выставлен ли флаг IsDead 
		if (health && health.IsDead)
		{
			// обездвиживаем игрока, отключая компоненты
			node.GetComponent<FirstPersonController>().Enabled = false;
			node.GetComponent<WeaponController>().Enabled = false;
			node.GetComponent<ShootInput>().Enabled = false;
		}
	}
}