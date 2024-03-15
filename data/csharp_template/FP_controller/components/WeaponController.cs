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

[Component(PropertyGuid = "f242c1ef0b63af70a079e9989c2f5f4b62a1c57b")]
public class WeaponController : Component
{
	public PlayerDummy shootingCamera = null;
	public ShootInput shootInput = null;
	public NodeDummy weaponMuzzle = null;
	public int damage = 1;

	// маска Intersection чтобы определить, в какие объекты могут попадать пули
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int mask = ~0;

	public void Shoot()
	{
		// задаем начало отрезка (p0) в позиции камеры и конец (p1) - в точке удаленной на 100 единиц в направлении взгляда камеры
		Vec3 p0 = shootingCamera.WorldPosition;
		Vec3 p1 = shootingCamera.WorldPosition + shootingCamera.GetWorldDirection() * 100;

		// создаем объект для хранения intersection-нормали
		WorldIntersectionNormal hitInfo = new WorldIntersectionNormal();
		// ищем первый объект, который пересекает отрезок (p0,p1)
		Unigine.Object hitObject = World.GetIntersection(p0, p1, mask, hitInfo);

		// если пересечение найдено
		if (hitObject)
		{
			// отрисовываем нормаль к поверхности в точке попадания при помощи Visualizer
			Visualizer.RenderVector(hitInfo.Point, hitInfo.Point + hitInfo.Normal, vec4.RED, 0.25f, false, 2.0f);
		}
	}

	private void Update()
	{
		// обработка пользовательского ввода: проверяем нажата ли клавиша “огонь”
		if (shootInput.IsShooting())
			Shoot();
	}
}