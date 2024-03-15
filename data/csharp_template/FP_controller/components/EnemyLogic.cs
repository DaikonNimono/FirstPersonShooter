using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif
#endregion

// определяем состояния врага
public enum EnemyLogicState
{
	Idle,
	Chase,
	Attack,
}

[Component(PropertyGuid = "5efec285bbcacbfd75d58238dff8fedd1ee54024")]
public class EnemyLogic : Component
{
	public Node player = null;
	public Node intersectionSocket = null;
	public float reachRadius = 0.5f;
	public float attackInnerRadius = 5.0f;
	public float attackOuterRadius = 7.0f;
	public float speed = 1.0f;
	public float rotationStiffness = 8.0f;
	public float routeRecalculationInterval = 3.0f;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int playerIntersectionMask = ~0;

	// инициализируем состояние врага
	private EnemyLogicState currentState = EnemyLogicState.Idle;

	private bool targetIsVisible;
	private Vec3 lastSeenPosition;
	private vec3 lastSeenDirection;
	private float lastSeenDistanceSqr;

	private BodyRigid bodyRigid = null;
	private WorldIntersection hitInfo = new WorldIntersection();
	private Node[] hitExcludes = new Node[2];

	private EnemyFireController fireController = null;
	// создаем очередь для точек пути
	private Queue<Vec3> calculatedRoute = new Queue<Vec3>();

	private PathRoute route = new PathRoute();
	private bool shouldUpdateRoute = true;
	private float lastCalculationTime = 0.0f;
	private bool IsTargetVisible()
	{
		Vec3 direction = (player.WorldPosition - intersectionSocket.WorldPosition);
		Vec3 p0 = intersectionSocket.WorldPosition;
		Vec3 p1 = p0 + direction;

		Unigine.Object hitObject = World.GetIntersection(p0, p1, playerIntersectionMask, hitExcludes, hitInfo);
		if (!hitObject)
			return false;

		return player.ID == hitObject.ID;
	}

	private void Init()
	{
		// задаем начальные значения параметров движения по пути внутри навигационной области
		route.Radius = 0.0f;
		route.Height = 1.0f;
		route.MaxAngle = 0.5f;

		bodyRigid = node.ObjectBodyRigid;
		hitExcludes[0] = node;
		hitExcludes[1] = node.GetChild(0);

		targetIsVisible = false;
		// берем компонент EnemyFireController
		fireController = node.GetComponent<EnemyFireController>();
		shouldUpdateRoute = true;
		lastCalculationTime = Game.Time;

	}

	private void Update()
	{
		UpdateTargetState();
		UpdateOrientation();
		UpdateRoute();

		// переключение состояний врага
		switch (currentState)
		{
			case EnemyLogicState.Idle: ProcessIdleState(); break;
			case EnemyLogicState.Chase: ProcessChaseState(); break;
			case EnemyLogicState.Attack: ProcessAttackState(); break;
		}

		// переключение цвета в зависимости от текущего состояния
		vec4 color = vec4.BLACK;
		switch (currentState)
		{
			case EnemyLogicState.Idle: color = vec4.BLUE; break;
			case EnemyLogicState.Chase: color = vec4.YELLOW; break;
			case EnemyLogicState.Attack: color = vec4.RED; break;
		}

		// визуализируем состояния врага
		Visualizer.RenderPoint3D(node.WorldPosition + vec3.UP * 2.0f, 0.25f, color);
		Visualizer.RenderPoint3D(node.WorldPosition + vec3.UP * 3.0f, 0.25f, IsTargetVisible() ? vec4.GREEN : vec4.RED);
		Visualizer.RenderPoint3D(lastSeenPosition, 0.1f, vec4.MAGENTA);

		// визуализируем радиус атаки
		Visualizer.RenderSphere(attackInnerRadius, node.WorldTransform, vec4.RED);
		Visualizer.RenderSphere(attackOuterRadius, node.WorldTransform, vec4.RED);

		// визуализируем точки маршрута
		foreach (vec3 route_point in calculatedRoute)
			Visualizer.RenderPoint3D(route_point + vec3.UP, 0.25f, vec4.BLACK);

	}
	private void UpdateRoute()
	{
		if (Game.Time - lastCalculationTime < routeRecalculationInterval)
			return;

		if (shouldUpdateRoute)
		{
			// рассчитываем путь до игрока
			route.Create2D(node.WorldPosition, lastSeenPosition, 1);
			shouldUpdateRoute = false;
		}

		// если расчет пути окончен
		if (route.IsReady)
		{
			// проверяем, не достигнута ли целевая точка
			if (route.IsReached)
			{
				// очищаем очередь точек пути
				calculatedRoute.Clear();

				// добавляем все корневые точки в очередь
				for (int i = 1; i < route.NumPoints; ++i)
					calculatedRoute.Enqueue(route.GetPoint(i));

				shouldUpdateRoute = true;
				lastCalculationTime = Game.Time;
			}
			else
				// пересчитываем путь, если целевая точка не была достигнута
				shouldUpdateRoute = true;
		}
	}

	private void UpdateTargetState()
	{
		targetIsVisible = IsTargetVisible();
		if (targetIsVisible)
			lastSeenPosition = player.WorldPosition;

		lastSeenDirection = (vec3)(lastSeenPosition - node.WorldPosition);
		lastSeenDistanceSqr = lastSeenDirection.Length2;
		lastSeenDirection.Normalize();
	}

	private void UpdateOrientation()
	{
		vec3 direction = lastSeenDirection;
		direction.z = 0.0f;

		quat targetRotation = new quat(MathLib.SetTo(vec3.ZERO, direction.Normalized, vec3.UP, MathLib.AXIS.Y));
		quat currentRotation = node.GetWorldRotation();

		currentRotation = MathLib.Slerp(currentRotation, targetRotation, Game.IFps * rotationStiffness);
		node.SetWorldRotation(currentRotation);
	}

	private void ProcessIdleState()
	{
		// если видна цель (игрок) - переход Бездействие -> Преследование (Chase)
		if (targetIsVisible)
			currentState = EnemyLogicState.Chase;
	}

	private void ProcessChaseState()
	{

		vec3 currentVelocity = bodyRigid.LinearVelocity;
		currentVelocity.x = 0.0f;
		currentVelocity.y = 0.0f;
		if (calculatedRoute.Count > 0)
		{
			float distanceToTargetSqr = (float)(calculatedRoute.Peek() - node.WorldPosition).Length2;

			bool targetReached = (distanceToTargetSqr < reachRadius * reachRadius);
			if (targetReached)
				calculatedRoute.Dequeue();

			if (calculatedRoute.Count > 0)
			{
				vec3 direction = (vec3)(calculatedRoute.Peek() - node.WorldPosition);
				direction.z = 0.0f;
				direction.Normalize();
				currentVelocity.x = direction.x * speed;
				currentVelocity.y = direction.y * speed;
			}

		}

		// если цель не видна - переход Преследование -> Бездействие
		if (!targetIsVisible)
			currentState = EnemyLogicState.Idle;

		// проверка дистанции и переход Преследование -> Атака
		else if (lastSeenDistanceSqr < attackInnerRadius * attackInnerRadius)
		{
			currentState = EnemyLogicState.Attack;
			currentVelocity.x = 0.0f;
			currentVelocity.y = 0.0f;
			// начинаем стрельбу
			if (fireController)
				fireController.StartFiring();
		}

		bodyRigid.LinearVelocity = currentVelocity;
	}

	private void ProcessAttackState()
	{
		// проверка дистанции и переход Атака -> Преследование
		if (!targetIsVisible || lastSeenDistanceSqr > attackOuterRadius * attackOuterRadius)
		{
			currentState = EnemyLogicState.Chase;
			// прекращаем стрельбу
			if (fireController)
				fireController.StopFiring();
		}
	}
}