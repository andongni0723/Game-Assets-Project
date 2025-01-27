using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class MenuButtonController : MonoBehaviour
{
    public Transform disappearPoint;
    public GameObject mainButtonsGroup;
    public GameObject toPlayButtonsGroup;

    [FormerlySerializedAs("pvpForm")]
    [Header("Setting")] 
    
    [Header("Battle")] 
    [SceneName] public string battleForm;
    [SceneName] public string battleTo;
    
    



    #region Button Event

    public void PlayButton()
    {
        StartCoroutine(ButtonsGroupFade(mainButtonsGroup, toPlayButtonsGroup));
    }

    public void PVPButton()
    {
        ModeManager.Instance.ChangeGameMode(GameMode.PVP);
        TeleportManager.Instance.Transition(battleForm, battleTo);
    }
    
    public void PVEButton()
    {
        ModeManager.Instance.ChangeGameMode(GameMode.PVE);
        TeleportManager.Instance.Transition(battleForm, battleTo);
    }

    public void BackButton()
    {
        StartCoroutine(ButtonsGroupFade(toPlayButtonsGroup, mainButtonsGroup));
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    #endregion

    IEnumerator ButtonsGroupFade(GameObject group1,GameObject group2)
    {
        group2.transform.position = disappearPoint.position;
        group2.SetActive(true);
        
        // Animation
        Sequence sequence = DOTween.Sequence();
        sequence.Append(group1.transform.DOLocalMoveX(disappearPoint.position.x, 0.2f));
        sequence.Append(group2.transform.DOLocalMoveX(0, 0.2f));
        
        yield return sequence.WaitForCompletion();
        
        group1.SetActive(false);

    }
}
