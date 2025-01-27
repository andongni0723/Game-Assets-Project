using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class MainCanvasManager : MonoBehaviour
{
    void Awake()
    {

        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();

        float screenWidthScale = Screen.width / canvasScaler.referenceResolution.x;
        float screenHeightScale = Screen.height / canvasScaler.referenceResolution.y;

        canvasScaler.matchWidthOrHeight = screenWidthScale > screenHeightScale ? 1 : 0;

        // Debug.Log(Screen.currentResolution);
        // //1920 x 1080 @ 60Hz

        // //Output the current screen window width in the console
        // Debug.Log("Screen Width : " + Screen.width);
        // //Screen Width : 1920

        // //Output the current screen window height in the console
        // Debug.Log("Screen Height : " + Screen.height);
        // //Screen Height : 1080
    }

    #region Event
    private void OnEnable() {
        EventHanlder.PlayerHurt += CameraShake;
        EventHanlder.EnemyHurt += CameraShake;
    }
    private void OnDisable() {
        EventHanlder.PlayerHurt -= CameraShake;
        EventHanlder.EnemyHurt -= CameraShake;
    }

    private void CameraShake(CardDetail_SO data)
    {
        transform.DOShakePosition(0.5f);
    }
    #endregion
}
