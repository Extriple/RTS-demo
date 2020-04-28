using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Unit, ISelectable
{
   

    public void SetSelected(bool selected)
    {
        //Chcemy włączyć lub wyłączyć pasek życia w zależności czy jest to selected czy nie 
        healthBar.gameObject.SetActive(selected);
    }

    void Command(Vector3 destination)
    {
        nav.SetDestination(destination);
        task = Task.move;
        target = null;
    }
    void Command(Soldier soldierToFollow)
    {
        target = soldierToFollow.transform;
        task = Task.follow;
    }
    void Command(Dragon dragonToKill)
    {
        //todo
    }


}
