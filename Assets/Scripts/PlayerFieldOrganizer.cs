using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerFieldOrganizer
{
    public const float PlayerFieldSpacing = 10f;
    private List<PlayerField> playerFields = new List<PlayerField>();

    // Returns the middle position along the X-axis.
    public float MiddleX { 
        get 
        {
            return playerFields.Count > 0 ? 
                (playerFields.Count - 1) * PlayerFieldSpacing / 2f : 0f;
        }
    }
    
    public void Add(PlayerField field)
    {
        float xPos = playerFields.Count * PlayerFieldSpacing;
        field.TargetPosition = Vector3.right * xPos;
        field.transform.position = field.TargetPosition + Vector3.up * 10f;
        playerFields.Add(field);
    }

    public void Remove(PlayerField field)
    {
        playerFields.Remove(field);
        for (int i = 0; i < playerFields.Count; ++i)
        {
            playerFields[i].TargetPosition = Vector3.right * i * PlayerFieldSpacing;
        }
    }
}
