using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;
#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

[Component(PropertyGuid = "19c5bafbe8a7932cdd4890d4a298a355b4d1dec7")]
public class VFXController : Component
{
	// NodeReference для эффектов вспышки выстрела и попадания
	[ParameterFile(Filter = ".node")]
	public string hitPrefab = null;

	[ParameterFile(Filter = ".node")]
	public string muzzleFlashPrefab = null;

	public void OnShoot(Mat4 transform)
	{
		// если не задан NodeReference эффекта вспышки, ничего не делаем
		if (string.IsNullOrEmpty(muzzleFlashPrefab))
			return;

		// загружаем NodeReference эффекта выстрела
		Node muzzleFlashVFX = World.LoadNode(muzzleFlashPrefab);
		// устанавливаем положение вспышки на указанные координаты дула пистолета
		muzzleFlashVFX.WorldTransform = transform;
	}

	public void OnHit(Vec3 hitPoint, vec3 hitNormal, Unigine.Object hitObject)
	{
		// если нода эффекта попадания не указана ничего не делаем
		if (string.IsNullOrEmpty(hitPrefab))
			return;

		// загружаем ноду эффекта попадания из файла
		Node hitVFX = World.LoadNode(hitPrefab);
		// устанавливаем загруженную ноду в указанную точку попадания и разворачиваем ее в направлении вектора нормали
		hitVFX.Parent = hitObject;
		hitVFX.WorldPosition = hitPoint;
		hitVFX.SetWorldDirection(hitNormal, vec3.UP, MathLib.AXIS.Y);
	}
}