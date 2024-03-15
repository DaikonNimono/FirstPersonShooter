using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "6b62a5653e3463f1423407e64c4abca756dadd51")]
public class HUD : Component
{
	// параметры прицела
	public AssetLink crosshairImage = null;
	public int crosshairSize = 16;

	private WidgetSprite sprite = null;
	private Gui screenGui = null;
	private ivec2 prev_size = new ivec2(0, 0);


	private void Init()
	{
		// получаем текущий экранный GUI
		screenGui = Gui.GetCurrent();

		// создаем виджет WidgetSprite для прицела
		sprite = new WidgetSprite(screenGui, crosshairImage.AbsolutePath);
		// задаем размер спрайта
		sprite.Width = crosshairSize;
		sprite.Height = crosshairSize;

		// добавляем спрайт к GUI так, чтобы он всегда был посередине экрана и поверх всех остальных виджетов
		screenGui.AddChild(sprite, Gui.ALIGN_CENTER | Gui.ALIGN_OVERLAP);
		// привязываем время жизни виджета к миру
		sprite.Lifetime = Widget.LIFETIME.WORLD;
	}

	private void Update()
	{
		ivec2 new_size = screenGui.Size;
		if (prev_size != new_size)
		{
			screenGui.RemoveChild(sprite);
			screenGui.AddChild(sprite, Gui.ALIGN_CENTER | Gui.ALIGN_OVERLAP);
		}
		prev_size = new_size;
	}
}