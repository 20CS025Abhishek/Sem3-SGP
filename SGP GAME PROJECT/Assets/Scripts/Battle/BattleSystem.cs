﻿/*
	Module name BattleDialogueBox
	Module creation date - 04-Sep-2021
	@author: Abhishek Kayasth
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
	[SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;

	[SerializeField] BattleDialogueBox dialogueBox;
	[SerializeField] PartyScreen partyScreen;

	public event Action<bool> OnBattleOver;

	BattleState state;
	int currentAction;
	int currentMove;
	int currentMember;

	PokemonParty playerParty;
	Pokemon wildPokemon;

	public void StartBattle(PokemonParty playerParty , Pokemon wildPokemon )
	{
		this.playerParty = playerParty;
		this.wildPokemon = wildPokemon;
		StartCoroutine(SetupBattle());
	}

	public IEnumerator SetupBattle()
	{
		playerUnit.Setup(playerParty.GetHealthyPokemon());
		enemyUnit.Setup(wildPokemon);

		partyScreen.Init();

		dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

		yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");

		ChooseFirstTurn();
	}

	void ChooseFirstTurn() // Add this function in switch pokemon coroutine too
	{
		if(playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
			ActionSelection();
		else	
			StartCoroutine(EnemyMove());
	}

	void BattleOver(bool won)
	{
		state = BattleState.BattleOver;
		playerParty.Pokemons.ForEach( p => p.OnBattleOver());
		OnBattleOver(won);
	}

	void ActionSelection()
	{
		state = BattleState.ActionSelection;
		dialogueBox.SetDialogue("Choose an action");
		dialogueBox.EnableActionSelector(true);
	}
	void OpenPartyScreen()
	{
		state = BattleState.PartyScreen;
		partyScreen.SetPartyData(playerParty.Pokemons);
		partyScreen.gameObject.SetActive(true);
	}

	void MoveSelection()
	{
		state = BattleState.MoveSelection;
		dialogueBox.EnableActionSelector(false);
		dialogueBox.EnableDialogueText(false);
		dialogueBox.EnableMoveSelector(true);
	}

	IEnumerator PlayerMove()
	{
		state = BattleState.PerformMove;

		var move = playerUnit.Pokemon.Moves[currentMove];
		yield return RunMove(playerUnit, enemyUnit, move);

		if(state == BattleState.PerformMove)
			StartCoroutine(EnemyMove());
	}

	IEnumerator EnemyMove()
	{
		state = BattleState.PerformMove;

		var move = enemyUnit.Pokemon.GetRandomMove();
		yield return RunMove(enemyUnit, playerUnit, move);

		if(state == BattleState.PerformMove)
			ActionSelection();
	}

	IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
	{
		move.PP--;
		yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

		sourceUnit.PlayAttackAnimation();
		yield return new WaitForSeconds(1f);
		targetUnit.PlayHitAnimation();

		if(move.Base.Category == MoveCategory.Status)
		{
			yield return RunMoveEffect(move, sourceUnit.Pokemon, targetUnit.Pokemon);
		}
		else
		{
			var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
			yield return targetUnit.Hud.UpdateHP();
			yield return ShowDamageDetails(damageDetails);
		}

		if (targetUnit.Pokemon.HP <= 0)
		{
			yield return dialogueBox.TypeDialogue($"Foe {targetUnit.Pokemon.Base.Name} fainted");
			targetUnit.PlayFaintAnimation();

			yield return new WaitForSeconds(2f);

			CheckBattleOver(targetUnit);
		}
	}

	IEnumerator RunMoveEffect(Move move, Pokemon source, Pokemon target)
	{
		var effects = move.Base.Effects;
			if(effects.Boosts != null)
			{
				if(move.Base.Target == MoveTarget.Self)
					source.ApplyBoosts(effects.Boosts);
				else
					target.ApplyBoosts(effects.Boosts);
			}

			yield return ShowStatusChanges(source);
			yield return ShowStatusChanges(target);
	}

	IEnumerator ShowStatusChanges(Pokemon pokemon)
	{
		while(pokemon.StatusChanges.Count > 0)
		{
			var message = pokemon.StatusChanges.Dequeue();
			yield return dialogueBox.TypeDialogue(message);
		}
	}

	void CheckBattleOver(BattleUnit faintedUnit)
	{
		if(faintedUnit.IsPlayerUnit)
		{
			var nextPokemon = playerParty.GetHealthyPokemon();
			if(nextPokemon != null)
				OpenPartyScreen();
			else
				BattleOver(false);
		}
		else
			BattleOver(true);
			
	}

	IEnumerator ShowDamageDetails(DamageDetails damageDetails)
	{
		if (damageDetails.Critical > 1f)
			yield return dialogueBox.TypeDialogue("A critical hit!");

		if(damageDetails.TypeEffectiveness > 1f)
			yield return dialogueBox.TypeDialogue("It's super effective!");
		else if(damageDetails.TypeEffectiveness < 1f)
			yield return dialogueBox.TypeDialogue("It's not very effective");
	}

	public void HandleUpdate()
	{
		if (state == BattleState.ActionSelection)
		{
			HandleActionSelection();
		}
		else if (state == BattleState.MoveSelection)
		{
			HandleMoveSelection();
		}
		else if (state == BattleState.PartyScreen)
		{
			HandlePartySelection();
		}
	}

	void HandleActionSelection()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
			++currentAction;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--currentAction;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			currentAction+=2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			currentAction-=2;

		currentAction = Mathf.Clamp(currentAction, 0, 3);
		dialogueBox.UpdateActionSelection(currentAction);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			if (currentAction == 0)
			{
				// Fight
				MoveSelection();
			}
			else if (currentAction == 1)
			{
				// Bag
			}
			else if (currentAction == 2)
			{
				// Party
				OpenPartyScreen();
			}
			else if (currentAction == 3)
			{
				// Run
			}
		}
	}

	void HandleMoveSelection()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
			++currentMove;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--currentMove;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			currentMove+=2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			currentMove-=2;

		currentMove = Mathf.Clamp(currentMove, 0 , playerUnit.Pokemon.Moves.Count - 1);
		dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			dialogueBox.EnableMoveSelector(false);
			dialogueBox.EnableDialogueText(true);
			StartCoroutine(PlayerMove());
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			dialogueBox.EnableMoveSelector(false);
			dialogueBox.EnableDialogueText(true);
			ActionSelection();
		}
	}

	void HandlePartySelection()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
			++currentMember;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--currentMember;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			currentMember+=2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			currentMember-=2;

		currentMember = Mathf.Clamp(currentMember, 0 , playerParty.Pokemons.Count - 1);

		partyScreen.UpdateMemberSelection(currentMember);

		if(Input.GetKeyDown(KeyCode.Z))
		{
			var selectedMember = playerParty.Pokemons[currentMember];
			if(selectedMember.HP <= 0)
			{
				partyScreen.SetMessageText("You can't send out a fainted Pokemon");
				return;
			}
			if(selectedMember == playerUnit.Pokemon)
			{
				partyScreen.SetMessageText($"{selectedMember.Base.Name} is already in battle");
				return;
			}

			partyScreen	.gameObject.SetActive(false);
			state = BattleState.Busy;
			StartCoroutine(SwitchPokemon(selectedMember));
		}
		else if(Input.GetKeyDown(KeyCode.X))
		{
			partyScreen	.gameObject.SetActive(false);
			ActionSelection();
		}
	}

	IEnumerator SwitchPokemon(Pokemon newPokemon)
	{
		dialogueBox.EnableActionSelector(false);
		if(playerUnit.Pokemon.HP > 0)
		{
			yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
		}

		playerUnit.Setup(newPokemon);
		dialogueBox.SetMoveNames(newPokemon.Moves);

		yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");

		StartCoroutine(EnemyMove());
	}
}