using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVPGameManager : GameManager
{
    protected override IEnumerator LoopGameStepAction()
    {
        WaitForSeconds waitStepStart = new WaitForSeconds(1);
        while (true)
        {
            switch (gameStep)
            {
                case GameStep.StepStart:
                    ChangeGameStep(GameStep.PlayerStep);
                    break;

                case GameStep.PlayerStep:
                    ChangeGameStep(GameStep.CommonStep);
                    ChangeCardsOnStepStart(currentCharacter);
                    yield return waitStepStart;//FIXME
                    EventHanlder.CallPlayerStepAddCard();
                    break;

                case GameStep.CommonStep:
                    if (CommonCardActionList.Count != 0)
                        yield return StartCoroutine(ExecuteCardActionList(CommonCardActionList));
                    break;

                case GameStep.EnemySettlement:
                    //TODO: future to enemy 
                    currentCharacter = Character.Enemy;
                    yield return StartCoroutine(ExecuteCardActionList(EnemySettlementCardActionList));
                    yield return StartCoroutine(ExecuteStatusEffectActionList(SettlementHurtStatusEffectActionList));
                    enemyLastHurtNum = enemyHurtSumCurrent;
                    enemyHurtSumCurrent = 0;
                    ChangeGameStep(GameStep.EnemyStep);
                    break;

                case GameStep.EnemyStep:
                    ChangeGameStep(GameStep.CommonStep);
                    yield return waitStepStart;//FIXME
                    ChangeCardsOnStepStart(currentCharacter);
                    EventHanlder.CallPlayerStepAddCard(); 
                    break;


                case GameStep.PlayerSettlement:
                    currentCharacter = Character.Player;
                    yield return StartCoroutine(ExecuteCardActionList(PlayerSettlementCardActionList));
                    yield return StartCoroutine(ExecuteStatusEffectActionList(SettlementHurtStatusEffectActionList));
                    playerLastHurtNum = playerHurtSumCurrent;
                    playerHurtSumCurrent = 0;
                    ChangeGameStep(GameStep.StepEnd); //TODO: future
                    break;

                case GameStep.StepEnd:
                    ChangeGameStep(GameStep.StepStart); //TODO: future
                    break;
            }
            yield return null;
        }
    }

    protected override void OnCommandStepEnd()
    {
        switch(currentCharacter)
        {
            case Character.Player:
                ChangeGameStep(GameStep.EnemySettlement);
                break;
            
            case Character.Enemy:
                ChangeGameStep(GameStep.PlayerSettlement);
                break;
        }
    }
}
