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
    
    public Slider heavyAttackSlider;

    public BattleState state;

    public bool actionTaken = false;

    private bool heavyAttackUsed = false;

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
        
        heavyAttackSlider.gameObject.SetActive(false);

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

        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
            yield break;
        }

        actionTaken = false;
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator HeavyAttack()
    {
        dialogueText.text = "Charging heavy attack...";

        heavyAttackSlider.gameObject.SetActive(true);
        heavyAttackSlider.value = 0;

        float chargeTime = 3f;
        float timer = 0f;

        while (timer < chargeTime)
        {
            timer += Time.deltaTime;
            heavyAttackSlider.value = timer / chargeTime;
            yield return null;
        }

        heavyAttackSlider.gameObject.SetActive(false);

        dialogueText.text = "You unleash a powerful heavy attack!";
        yield return new WaitForSeconds(1f);

        bool isDead = enemyUnit.TakeDamage(15);
        enemyHUD.SetHp(enemyUnit.currentHealth);

        dialogueText.text = "Your heavy attack dealt 15 damage!";
        heavyAttackUsed = true;

        yield return new WaitForSeconds(2f);

        if (isDead)
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

        if (playerUnit.isBlocking)
        {
            int blockChance = Random.Range(0, 2);
            if (blockChance == 1)
            {
                dialogueText.text = "You successfully blocked the attack!";
            }
            else
            {
                dialogueText.text = "You tried to block, but failed!";
                bool isDead = playerUnit.TakeDamage(randomDamage);
                playerHUD.SetHp(playerUnit.currentHealth);

                if (isDead)
                {
                    state = BattleState.LOST;
                    StartCoroutine(EndBattle());
                    yield break;
                }
            }
            playerUnit.isBlocking = false;
        }
        else
        {
            bool isDead = playerUnit.TakeDamage(randomDamage);
            dialogueText.text = enemyUnit.unitName + " does " + randomDamage + " damage!";
            playerHUD.SetHp(playerUnit.currentHealth);

            if (isDead)
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
                yield break;
            }
        }

        yield return new WaitForSeconds(1f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            dialogueText.text = "You won! Next level starts in 5 seconds";

            yield return new WaitForSeconds(5f);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if (state == BattleState.LOST)
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

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN || actionTaken)
            return;

        actionTaken = true;
        StartCoroutine(PlayerAttack());
    }

    public void OnHeavyAttackButton()
    {
        if (state != BattleState.PLAYERTURN || actionTaken)
            return;

        if (heavyAttackUsed)
        {
            dialogueText.text = "You can only use the Heavy Attack once!";
            return;
        }

        actionTaken = true;
        StartCoroutine(HeavyAttack());
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
            dialogueText.text = "Your HP is full! You can't heal now.";

            yield return new WaitForSeconds(2f);

            actionTaken = false;
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN || actionTaken)
            return;

        actionTaken = true;
        StartCoroutine(PlayerHeal());
    }

    public void OnBlockButton()
    {
        if (state != BattleState.PLAYERTURN || actionTaken)
            return;

        StartCoroutine(PlayerBlock());
    }

    IEnumerator PlayerBlock()
    {
        dialogueText.text = "You prepare to block!";
        playerUnit.isBlocking = true;

        yield return new WaitForSeconds(1f);

        dialogueText.text = "You can still attack or heal!";
    }
}
