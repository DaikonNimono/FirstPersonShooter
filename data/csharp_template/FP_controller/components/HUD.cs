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

	private WidgetLabel label = null;

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

		// добавляем виджет WidgetLabel для отображения здоровья игрока, устанавливаем его положение размер шрифта
		label = new WidgetLabel(screenGui, "");
		label.FontSize = 50;
		label.SetPosition(10, 10);

		// добавляем виджет к GUI
		screenGui.AddChild(label, Gui.ALIGN_CENTER | Gui.ALIGN_OVERLAP);
		// привязываем время жизни виджета к миру
		label.Lifetime = Widget.LIFETIME.WORLD;
	}

	private void Update()
	{
		ivec2 new_size = screenGui.Size;
		if (prev_size != new_size)
		{
			screenGui.RemoveChild(sprite);
			screenGui.AddChild(sprite, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		}
		prev_size = new_size;
	}

	// обновление текущего уровня здоровья игрока
	public void UpdateHealthInfo(int health)
	{
		label.Text = "Health: " + health.ToString();
	}

	// обновление текущего уровня здоровья игрока
	public void DisplayStateMessage(GameState state)
	{
		// добавляем виджет WidgetLabel для отображения финального сообщение о результате игры, устанавливаем размер и цвет шрифта
		WidgetLabel end_message = new WidgetLabel(screenGui, (state == GameState.Win) ? "ПОБЕДА!" : "ВЫ ПРОИГРАЛИ!");
		end_message.FontSize = 100;
		end_message.FontColor = vec4.RED;
		screenGui.AddChild(end_message, Gui.ALIGN_CENTER | Gui.ALIGN_OVERLAP);
		// привязываем время жизни виджета к миру
		end_message.Lifetime = Widget.LIFETIME.WORLD;

		// завершаем процесс
		ComponentSystem.FindComponentInWorld<GameController>().Enabled = false;
	}
}