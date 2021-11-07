﻿/*
    @author : Mitren Kadiwala
*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    //For crearing refrence
    public List<Pokemon> Pokemons {
        get {
            return pokemons;
        }
        set {
            pokemons = value;
        }
    }

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
    // Pokemons List In Player Party 
    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
        }
        else
        {
            // TODO: add to the PC ones that's implemented
        }
    }
}
