using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "19086705f07c9e76dd45800696a0f47c5ef21cf1")]
public class ShootInput : Component
{
	public bool IsShooting()
	{
		// возвращаем текущее состояние левой кнопки мыши и проверяем захват мыши в окне
		return Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT) && Input.MouseGrab; ;
	}
}