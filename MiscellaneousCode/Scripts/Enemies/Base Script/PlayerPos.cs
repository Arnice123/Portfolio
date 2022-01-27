 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPos : MonoBehaviour
{
    // this function will check to see if the enemy has seen the player

    public static bool FindPlayerPos(float inRange, Transform playerPos, float inView, GameObject self, bool hasSeenPlayer)
    {
        // the distance diference between the player and enemy

        float distDif = Vector3.Distance(playerPos.position, self.transform.position);

        // if the player is in range and in view then it has seen the player

        if (distDif <= inRange && PlayerIsInSight(inView, playerPos, self) && !hasSeenPlayer)
        {
            // return true

            hasSeenPlayer = true;            
            return hasSeenPlayer;
            
        }

        // if you already have seen the player then just go to them

        if (distDif <= inRange && hasSeenPlayer)
        {
            // look at the player and then return true

            self.transform.LookAt(playerPos.position);            
            return true;
        }        
        return false;
    }

    // this will check to see if the enemy is in view

    public static bool PlayerIsInSight(float inView, Transform playerPos, GameObject self)
    {
        // calculating the angle

        Vector3 targetDir = playerPos.position - self.transform.position;
        targetDir.y = 0f;
        Vector3 forward = self.transform.forward;
        float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        float selfHeight = self.GetComponent<Collider>().bounds.size.y;

        if (angle <= inView && angle >= -inView && playerPos.position.y <= self.transform.position.y + selfHeight/2 + selfHeight && playerPos.position.y >= (self.transform.position.y - selfHeight) - selfHeight / 2)
        {
            return true;
        }
        return false;
    }
}
