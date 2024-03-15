using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "d798d617fcbdfc8a6e2dc2610bad784cb198573b")]
public class Lifetime : Component
{
	[ShowInEditor][Parameter(Tooltip = "Время жизни объекта (в секундах)")]
	private float lifeTime = 1.0f;

	private float startTime = 0.0f;

	void Init()
	{
		// запоминаем время создания объекта
		startTime = Game.Time;
	}

	void Update()
	{
		// ждем пока не истечет время жизни ноды, затем удаляем ее
		if (Game.Time - startTime > lifeTime)
			node.DeleteLater();
	}
}
