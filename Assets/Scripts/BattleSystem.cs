using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{

	public GameObject playerPrefab;
	public GameObject enemyPrefab;

	public Transform playerBattleStation;
	public Transform enemyBattleStation;

	Unit playerUnit;
	Unit enemyUnit;

	public TMP_Text dialogueText;

	public BattleHud playerHUD;
	public BattleHud enemyHUD;

	public BattleState state;
	
    void Start()
    {
		state = BattleState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
		playerUnit = playerGO.GetComponent<Unit>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		enemyUnit = enemyGO.GetComponent<Unit>();

		dialogueText.text = enemyUnit.unitName + " is een enemy!";

		playerHUD.setHUD(playerUnit);
		enemyHUD.setHUD(enemyUnit);

		yield return new WaitForSeconds(2f);

		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

	IEnumerator PlayerAttack()
	{
		int action = Random.Range(0, 2);
		Scene activeScene = SceneManager.GetActiveScene();

		if (activeScene.name == "Level3")
		{
			if (action == 1)
			{
				dialogueText.text = "Enemy blokkeert de attack!!";
				yield return new WaitForSeconds(2f);
				state = BattleState.ENEMYTURN;
				StartCoroutine(EnemyTurn());
				yield break;
			}
		}
		
		bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
		enemyHUD.SetHp(enemyUnit.currentHealth);
		dialogueText.text = "De aanval is succesvol!";
		
		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			state = BattleState.WON;
			EndBattle();
			yield break;
		} 
		
		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	IEnumerator EnemyTurn()
	{
		
		int action = Random.Range(0, 2);
		Scene activeScene = SceneManager.GetActiveScene();
		
		
		if (activeScene.name == "Level2" || activeScene.name == "Level3")
		{
			if (action == 1)
			{
				dialogueText.text = enemyUnit.unitName + " healt zichzelf!";
				enemyUnit.Heal(5);
				enemyHUD.SetHp(enemyUnit.currentHealth);
				yield return new WaitForSeconds(1f);
				state = BattleState.PLAYERTURN;
				PlayerTurn();
				yield break;
			}
		}
		
		int randomDamage = enemyUnit.GenerateRandomDamage(1, 11);
		bool isDead = playerUnit.TakeDamage(randomDamage);
		dialogueText.text = enemyUnit.unitName + " doet " + randomDamage + " damage!";
		playerHUD.SetHp(playerUnit.currentHealth);
		yield return new WaitForSeconds(1f);
		
		if(isDead)
		{
			state = BattleState.LOST;
			EndBattle();
			yield break;
		} 
		
		state = BattleState.PLAYERTURN;
		PlayerTurn();

	}

	void EndBattle()
	{
		if(state == BattleState.WON)
		{
			dialogueText.text = "Je hebt gewonnen!";
		} else if (state == BattleState.LOST)
		{
			dialogueText.text = "Je hebt verloren!";
		}
	}

	void PlayerTurn()
	{
		dialogueText.text = "Kies een actie:";
	}

	IEnumerator PlayerHeal()
	{
		playerUnit.Heal(5);

		playerHUD.SetHp(playerUnit.currentHealth);
		dialogueText.text = "Je hebt gehealed";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}

}
