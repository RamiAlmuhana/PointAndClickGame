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
	
	public bool actionTaken = false;

	
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

		dialogueText.text = enemyUnit.unitName + " is an enemy!";

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
				dialogueText.text = "Enemy blocks your attack!!";
				yield return new WaitForSeconds(2f);
				state = BattleState.ENEMYTURN;
				StartCoroutine(EnemyTurn());
				actionTaken = false;
				yield break;
			}
		}
		
		bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
		enemyHUD.SetHp(enemyUnit.currentHealth);
		dialogueText.text = "The attack is successful!";
		
		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			state = BattleState.WON;
			StartCoroutine(EndBattle());
			yield break;
		} 
		
		actionTaken = false;
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
				if (enemyUnit.currentHealth < enemyUnit.maxHealth)
				{
					dialogueText.text = enemyUnit.unitName + " healed himself!";
					enemyUnit.Heal(5);
					enemyHUD.SetHp(enemyUnit.currentHealth);
					yield return new WaitForSeconds(1f);
					state = BattleState.PLAYERTURN;
					PlayerTurn();
					yield break;
				}
			}
		}
		
		int randomDamage = enemyUnit.GenerateRandomDamage(1, 11);
		bool isDead = playerUnit.TakeDamage(randomDamage);
		dialogueText.text = enemyUnit.unitName + " does " + randomDamage + " damage!";
		playerHUD.SetHp(playerUnit.currentHealth);
		yield return new WaitForSeconds(1f);
		
		if(isDead)
		{
			state = BattleState.LOST;
			StartCoroutine(EndBattle());
			yield break;
		} 
		
		state = BattleState.PLAYERTURN;
		PlayerTurn();

	}

	IEnumerator EndBattle()
	{
		if(state == BattleState.WON)
		{
			if (SceneManager.GetSceneByName("Level1") == SceneManager.GetActiveScene())
			{
				dialogueText.text = "You won! Next level start in 5 seconds";
			
				yield return new WaitForSeconds(5f);
			
				SceneManager.LoadScene("Level2");
			} else if (SceneManager.GetSceneByName("Level2") == SceneManager.GetActiveScene())
			{
				dialogueText.text = "You won! Next level start in 5 seconds";
				
				yield return new WaitForSeconds(5f);
			
				SceneManager.LoadScene("Level3");
			}else if (SceneManager.GetSceneByName("Level3") == SceneManager.GetActiveScene())
			{
				dialogueText.text = "You finished the game! Returning to the main menu";
				
				yield return new WaitForSeconds(5f);
			
				SceneManager.LoadScene("MainMenu");
			}

		} else if (state == BattleState.LOST)
		{
			dialogueText.text = "You lost! Level will restart in 5 seconds";
			
			yield return new WaitForSeconds(5f);
			
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	void PlayerTurn()
	{
		dialogueText.text = "Choose an action:";
	}

	IEnumerator PlayerHeal()
	{
		if (playerUnit.currentHealth < playerUnit.maxHealth)
		{
			playerUnit.Heal(5);

			playerHUD.SetHp(playerUnit.currentHealth);
			dialogueText.text = "You healed!";
			yield return new WaitForSeconds(2f);

			actionTaken = false;
			state = BattleState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
		}
		else
		{
			dialogueText.text = "Your full HP you cant heal!";
			
			yield return new WaitForSeconds(2f);
			
			actionTaken = false;
			state = BattleState.PLAYERTURN;
			PlayerTurn();
		}

	}

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN || actionTaken)
			return;
		
		actionTaken = true;
		StartCoroutine(PlayerAttack());
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN || actionTaken)
			return;

		actionTaken = true;
		StartCoroutine(PlayerHeal());
	}

}