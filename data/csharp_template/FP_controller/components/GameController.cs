using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

public enum GameState
{
	Gameplay,
	Win,
	Lose,
}

[Component(PropertyGuid = "aa9f1fea1e56f7a6da31878c2789e44171c877b4")]
public class GameController : Component
{
	public GameState state;
	public Player EndCamera = null; // Камера для финала игры

	public NodeDummy SpawnPoint = null;

	[ParameterFile]
	public AssetLink enemyPrefab = null;
	public int NumEnemies = 10;

	private int spawned_enemy_counter = 0;
	public float spawnInterval = 2.0f;
	private float currentTime = 0.0f;

	private void Init()
	{
		// Задаем начальное состояние игрового процесса
		state = GameState.Gameplay;
	}

	private void Update()
	{
		// если игра окончена
		if (state != GameState.Gameplay)
		{
			// переключаемся на камеру для финала игры
			Game.Player = EndCamera;
			// показываем сообщение об итоге игры в HUD
			ComponentSystem.FindComponentInWorld<HUD>().DisplayStateMessage(state);
		}
		else
		{
			// если врагов больше не осталось, переходим в состояние “Победа” (Win)
			if (!ComponentSystem.FindComponentInWorld<EnemyLogic>() && spawned_enemy_counter == NumEnemies)
				state = GameState.Win;
			// генерируем новых врагов (enemyPrefab) в заданной точке (SpawnPoint) 
			// с заданным интервалом времени (spawnInterval)
			if (spawned_enemy_counter < NumEnemies)
			{
				currentTime += Game.IFps;

				if (currentTime > spawnInterval)
				{
					currentTime -= spawnInterval;
					spawned_enemy_counter++;
					Node enemy = World.LoadNode(enemyPrefab.AbsolutePath);
					enemy.WorldTransform = SpawnPoint.WorldTransform;
				}
			}
		}
	}
}