using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using System;

public class BasicCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Compoment")]
    public CardDetail_SO cardDetail;
    public Vector4 halfPadding = new Vector4(0, 0, 90, 0); // half padding can let pointer easy Choose card
    public Vector4 zeroPadding = new Vector4(0, 0, 0, 0);  // zero padding can let pointer easy Drag card
    public Image image;

    public Transform originParent; // CardManager
    [SerializeField] public Transform playCardParent; // After play the card, will set parent to it

    [Header("Children")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardPayNumText;
    public Image cardImg;
    public TextMeshProUGUI cardDescriptionText;

    [Header("Card Data")]
    public int id;
    public float yPos;
    float targetCardYPos = 540;
    float scale;

    bool isDrag = false;

    private void Awake()
    {
        id = transform.GetSiblingIndex();
        originParent = GameObject.FindWithTag("CardManager").transform; // CardManager
        playCardParent = GameObject.FindWithTag("PlayCardParent").transform;
        image = GetComponent<Image>();
        scale = transform.localScale.x;

        // Get children gameObject
        cardNameText = transform.Find("card_name").GetComponent<TextMeshProUGUI>();
        cardPayNumText = transform.Find("card_pay").GetComponent<TextMeshProUGUI>();
        cardImg = transform.Find("card_img").GetComponent<Image>();
        cardDescriptionText = transform.Find("card_description").GetComponent<TextMeshProUGUI>();


        image.raycastPadding = halfPadding;
        //transform.DOMove(new Vector2(transform.position.x + 200, transform.position.y), 1);
    }

    private void Update()
    {
        if (transform.parent == originParent && isDrag)
        {
            transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// This method be call when cardManager set var done
    /// </summary>
    public void AfterInit()
    {
        UpdateDetail();
    }

    /// <summary>
    /// Update the detail about card (name, descripts...)
    /// </summary>
    private void UpdateDetail()
    {
        cardNameText.text = cardDetail.cardName;
        cardPayNumText.text = cardDetail.payCardNum.ToString();
        cardImg.sprite = cardDetail.cardSkillSprite;
        cardDescriptionText.text = cardDetail.cardDestription;
    }

    #region Event
    private void OnEnable()
    {
        EventHanlder.CardUpdatePosition += OnCardIDChange;
        EventHanlder.CardUpdatePosition += OnCardUpdatePosition; // Update Position
        EventHanlder.EndDragCardUpdateData += OnEndDragCardUpdateData; // Get CardDetail_SO
        EventHanlder.CancelPlayTheCard += OnCancelPlayTheCard; // Back to CardManager, and INIT
        EventHanlder.PayTheCardError += OnPayTheCardError; // Cancel pay card, back to CardManager
        EventHanlder.PayCardComplete += OnPayCardComplete; // Destroy card which paid OR played
    }
    private void OnDisable()
    {
        EventHanlder.CardUpdatePosition -= OnCardIDChange;
        EventHanlder.CardUpdatePosition -= OnCardUpdatePosition;
        EventHanlder.EndDragCardUpdateData -= OnEndDragCardUpdateData;
        EventHanlder.CancelPlayTheCard -= OnCancelPlayTheCard;
        EventHanlder.PayTheCardError -= OnPayTheCardError;
        EventHanlder.PayCardComplete -= OnPayCardComplete;
    }


    private void OnCardIDChange()
    {
        id = transform.GetSiblingIndex();
    }

    private void OnCardUpdatePosition()
    {
        // The card maybe by the card of played
        if (transform.parent == originParent)
        {
            transform.DOMove(GetComponentInParent<CardManager>().CardPositionList[id], 0.5f).OnComplete
            (
                () =>
                {
                    yPos = transform.position.y;
                }
            );
            // //The card maybe by payCard parent
            // if (transform.parent.TryGetComponent(out CardManager cardManager))
            // {
            //  
            // }
        }
    }
    private CardDetail_SO OnEndDragCardUpdateData()
    {
        return cardDetail;
    }

    private void OnCancelPlayTheCard()
    {
        transform.DOScale(scale * 1f, 0.3f);
        transform.parent = GameObject.FindGameObjectWithTag("CardManager").transform;
        //EventHanlder.CallCardUpdeatePosition();
        OnCardUpdatePosition();
        image.raycastPadding = halfPadding; //FIXME: padding
    }

    private void OnPayTheCardError(GameObject targetCard)
    {
        // This card haven't pay because the pay cards is enough
        if (transform.parent == null)
        {
            transform.parent = originParent;
        }
    }

    private void OnPayCardComplete()
    {
        // Destroy card which paid OR played
        if (transform.parent != originParent)
        {
            Destroy(gameObject);
        }
    }

    #endregion 


    #region Pointer Event
    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (!eventData.fullyExited) return; // Unity bug "IPointerEnter/Exit"
        if (GameManager.Instance.gameStep != GameStep.PayCardStep)
            transform.SetAsLastSibling(); // Let layer on first

        transform.DOScale(scale * 1.5f, 0.3f);
        //Debug.Log("enter");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDrag) return;

        //Show big card detail
        //TODO: The image will change to detail sprite "Card Detail Sprite" in future
        EventHanlder.CallCardOnClick(cardDetail);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!eventData.fullyExited) return; // Unity bug "IPointerEnter/Exit"
        transform.SetSiblingIndex(id); // Let layer become before
        transform.DOScale(scale * 1f, 0.3f);

        EventHanlder.CallCardOnClick(null);

        //Debug.Log("exit");
    }

    #endregion

    #region Drag Event
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent != originParent && isDrag) return; // Card was Paid

        isDrag = true;
        //transform.DOScale(scale * 0.3f, 0.3f);
        image.raycastPadding = zeroPadding;  //FIXME: padding
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (transform.parent != originParent) return; // Card was Paid
        //transform.position = eventData.position; //FIXME: move
        ScaleAtCardOnDrag(eventData);
        EventHanlder.CallCardOnDrag(cardDetail);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent != originParent) return; // Card was Paid

        EventAtCardEndDrag(eventData);
    }
    #endregion

    /// <summary>
    /// Card on drag, if card yPos out a distance, scale will become small (play), or big (canel play)
    /// </summary>
    /// <param name="eventData"></param>
    public void ScaleAtCardOnDrag(PointerEventData eventData)
    {

        // Change Scale
        if (eventData.position.y > targetCardYPos) // Play the card
        {
            transform.DOScale(scale * 0.3f, 0.2f);
        }
        else // Canel play the card
        {
            transform.DOScale(scale * 1f, 0.2f);
        }
    }

    public void EventAtCardEndDrag(PointerEventData eventData)
    {
        // Canel play the card
        if (eventData.position.y < targetCardYPos)
        {
            transform.DOScale(scale * 1f, 0.3f);
            OnCardUpdatePosition();
            image.raycastPadding = halfPadding; //FIXME: padding
        }
        else
        {
            var lastPos = transform.position; // Let card Position not be different after change parent
            transform.parent = null;
            transform.position = lastPos;

            // Is play card to attack, OR want pay card to let other card attack
            if (GameManager.Instance.gameStep == GameStep.PayCardStep)
            {
                EventHanlder.CallPayTheCard(gameObject);
            }
            else
            {
                transform.parent = playCardParent;
                EventHanlder.CallCardEndDrag();
            }
        }
        isDrag = false;
    }

}
