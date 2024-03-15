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

[Component(PropertyGuid = "88d6112b0032ace8365867c48dbe1958c29471f7")]
public class Bullet : Component
{
	public float speed = 10.0f;
	public int damage = 1;

	public AssetLink hitPrefab = null;

	[ParameterMask]
	public int intersectionMask = ~0;

	private WorldIntersectionNormal hitInfo = new WorldIntersectionNormal();

	private void Update()
	{
		// устанавливаем текущую позицию пули
		Vec3 currentPosition = node.WorldPosition;
		// устанавливаем направление движения пули вдоль оси Y
		vec3 currentDirection = node.GetWorldDirection(MathLib.AXIS.Y);

		// обновляем положение пули вдоль траектории в соответствии с заданной скоростью
		node.WorldPosition += currentDirection * speed * Game.IFps;

		// ищем пересечение траектории пули с каким-либо объектом
		Unigine.Object hitObject = World.GetIntersection(currentPosition, node.WorldPosition, intersectionMask, hitInfo);

		// если пересечений не найдено, ничего не делаем
		if (!hitObject)
			return;

		// иначе загружаем NodeReference с эффектом попадания
		Node hitEffect = World.LoadNode(hitPrefab.AbsolutePath);
		// устанавливаем NodeReference в точку попадания и ориентируем его по нормали к поверхности
		hitEffect.Parent = hitObject;
		hitEffect.WorldPosition = hitInfo.Point;
		hitEffect.SetWorldDirection(hitInfo.Normal, vec3.UP, MathLib.AXIS.Y);

		// удаляем пулю
		node.DeleteLater();
	}
}