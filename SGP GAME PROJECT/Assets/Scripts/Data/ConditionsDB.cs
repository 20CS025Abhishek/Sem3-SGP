﻿/*
	@author - Mitren Kadiwala
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{

    public static void Init()
    {
        // kvp -> Key value pair
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition  =kvp.Value;

            condition.Id = conditionId;
        }
    }
    
   public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() 
   {
       {
	   // Poison Damage And Dialogues
           ConditionID.psn,
           new Condition()
           {
               Name = "Poison",
               StartMessage = "has been poisoned",
               OnAfterTurn = (Pokemon pokemon) =>
               {
                   pokemon.UpdateHP(pokemon.MaxHp / 8);
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt due to poison");
               }
           }
       },
       {
	   // Burn Damage And Dialogues
           ConditionID.brn,
           new Condition()
           {
               Name = "Burn",
               StartMessage = "has been burned",
               OnAfterTurn = (Pokemon pokemon) =>
               {
                   pokemon.UpdateHP(pokemon.MaxHp / 16);
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt due to burn");
               }
           }
       },
       {
	   // Paralysed Damage And Dialogues
           ConditionID.par,
           new Condition()
           {
               Name = "Paralysed",
               StartMessage = "has been paralysed",
               OnBeforeMove = (Pokemon pokemon)=>
               {
                   if(Random.Range(1,4) == 1)
                   {
                       pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and can't move");
                       return false;
                   }
                   return true; 
               }
           }
       },
       {
	   // Freeze Damage And Dialogues
           ConditionID.frz,
           new Condition()
           {
               Name = "Freeze",
               StartMessage = "has been frozen",
               OnBeforeMove = (Pokemon pokemon)=>
               {
                   if(Random.Range(1,5) == 1)
                   {
                       pokemon.CureStatus();
                       pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore");
                       return true;
                   }
                   return false; 
               }
           }
       },
        {
	   // Sleep Damage And Dialogue
           ConditionID.slp,
           new Condition()
           {
               Name = "Sleep",
               StartMessage = "has fallen asleep",
               OnStart = (Pokemon pokemon)=>
               {
                   //The pokemon should sleep for 1-3 turns
                   pokemon.StatusTime = Random.Range(1,4);
                   Debug.Log($" will be asleep fpr {pokemon.StatusTime} moves ");  
               },
               OnBeforeMove = (Pokemon pokemon)=>
               {
                   if(pokemon.StatusTime <=0)
                   {
                       pokemon.CureStatus();
                       pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up");
                       return true;
                   }
                   pokemon.StatusTime--;
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping");
                   return false; 
                   
               }
           }
       },
       
       //Volatile Status Conditions
       {
           ConditionID.confusion,
           new Condition()
           {
               Name = "Confusion",
               StartMessage = "has been confused",
               OnStart = (Pokemon pokemon)=>
               {
                   //Confused for 1 - 4 turns
                   pokemon.VolatileStatusTime = Random.Range(1,5);
                   Debug.Log($" will be asleep fpr {pokemon.VolatileStatusTime} moves ");  
               },
               OnBeforeMove = (Pokemon pokemon)=>
               {
                   if(pokemon.VolatileStatusTime <= 0)
                   {
                       pokemon.CureVolatileStatus();
                       pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                       return true;
                   }
                   pokemon.VolatileStatusTime--;

                   //50% chance to do a move
                   if(Random.Range(1,3) == 1)
                   {
                       return true;
                   }

                   //Hurt by confusiom
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused");
                   pokemon.UpdateHP(pokemon.MaxHp / 8);
                   pokemon.StatusChanges.Enqueue($"It hurt itself due to confusion");
                   return false; 
                   
               }
           }
       }
   }; 
   //For Different Condition StatusBonus
   public static float GetStatusBonus(Condition condition) 
   {
       if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
         else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;    

      
   }    
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
