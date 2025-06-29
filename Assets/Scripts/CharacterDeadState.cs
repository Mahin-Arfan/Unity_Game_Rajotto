using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeadState : MonoBehaviour
{
    public bool isDead = false;
    public bool occupied = false;

    public bool IsDead()
    {
        return isDead == true;
    }

    public void Dead()
    {
        isDead = true;
    }

    public bool IsOccupied()
    {
        return occupied == true;
    }

    public void Occupied()
    {
        occupied = true;
    }

    public void unOccupied()
    {
        occupied = false;
    }
}
