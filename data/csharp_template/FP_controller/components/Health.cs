using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "51e877b3c29f323185bb02f9fb15bbc3f3022b4b")]
public class Health : Component
{
    public int health = 5;   // начальный уровень здоровья

    // флаг, проверяющий, не достиг ли текущий уровень здоровья 0
    public bool IsDead => health <= 0;

    public void TakeDamage(int damage)
    {
   	 // применяем ущерб
   	 health = MathLib.Max(health - damage, 0);
    }
}