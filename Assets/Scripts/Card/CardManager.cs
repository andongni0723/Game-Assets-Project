using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardManager : Singleton<CardManager>
{
    public List<Vector2> CardPositionList = new List<Vector2>();
    public List<Quaternion> CardRotationList = new List<Quaternion>();
    //public List<CardDetail_SO> CardDetailPrefabList = new List<CardDetail_SO>();
    public CharacterCards_SO Cards;
    public GameObject cardPrefabs;
    public GameObject discardAnimationPrefabs;
    public GameObject cardInstPoint;
    public GameObject discardPilePoint;

    public int stepStartPlayCardNum = 8;

    private float cardMoveX;
    private int instCardNameNum;

    [Header("Card Move Setting")]
    public float cardWidth = Screen.height / 7.55f; // 4KUHD  (286f)
    public float moveX;
    public float rotateNum = 4;
    public float rotateDownY = 3;

    [Header("Card Soft Setting")]
    public int maxCardNum = 5;


    protected override void Awake()
    {
        base.Awake();
        cardWidth = Screen.height / 7.55f; // 4KUHD  (286f)
    }


    #region Event
    private void OnEnable()
    {
        EventHanlder.ChangeCardsOnStepStart += ChangeCardsOnStepStart; // Change Cards with arg
        EventHanlder.PlayerStepAddCard += OnPlayerStepAddCard; // Init the some card
        EventHanlder.PayCardComplete += OnPayCardComplete; // Init the card position
    }
    private void OnDisable()
    {
        EventHanlder.ChangeCardsOnStepStart += ChangeCardsOnStepStart;
        EventHanlder.PlayerStepAddCard -= OnPlayerStepAddCard;
        EventHanlder.PayCardComplete -= OnPayCardComplete;
    }

    private void ChangeCardsOnStepStart(CharacterCards_SO data)
    {
        Cards = data;
    }

    private void OnPlayerStepAddCard()
    {
        for (int i = 0; i < stepStartPlayCardNum; i++)
        {
            AddCardButton();
        }
        
        EventHanlder.CallAddCardDone();
    }

    private void OnPayCardComplete()
    {
        ChangeCardPosition(transform.childCount);
    }
    #endregion

    private void OnTransformChildrenChanged()
    {
        // If Children Change (add or remove)
        if (CardPositionList.Count != transform.childCount)
            ChangeCardPosition(transform.childCount);
    }

    /// <summary>
    /// Change card position 
    /// </summary>
    /// <param name="_childNum"></param>
    private void ChangeCardPosition(int _childNum)
    {
        // Init
        CardPositionList.Clear();
        CardRotationList.Clear();

        cardMoveX = cardWidth + moveX;

        if (_childNum > maxCardNum)
        {
            //Debug.Log("bigger");//FIXME
            cardMoveX = cardMoveX / (1f + (_childNum - maxCardNum) / (maxCardNum - 1));
        }

        // If children count is even number, 
        // the card needs to move some right to keep cards is on center
        int odd = 1;
        odd = (_childNum % 2 == 0) ? 1 : 0;

        // the xPos of the leftest card
        float leftX = -(cardMoveX * (int)(_childNum / 2f)) + cardMoveX / 2f * odd;

        float z = rotateNum / (maxCardNum - 1f) * (_childNum - 1f);
        z = z > rotateNum ? rotateNum : z;

        for (int i = 0; i < _childNum; i++)
        {
            float z1 = ((float)_childNum - 1f);
            z1 = z1 == 0 ? 1 : z1;
            float rotateZ = z - (z * 2f) / z1 * i;

            CardRotationList.Add(Quaternion.Euler(0f, 0f, rotateZ));


            // Add Position to List
            CardPositionList.Add(new Vector2(transform.position.x + leftX + cardMoveX * i, transform.position.y - (Mathf.Abs(rotateZ)) * rotateDownY));
        }

        EventHanlder.CallCardUpdeatePosition();
    }


    /// <summary>
    /// Give the CardDetail to inst the card
    /// </summary>
    /// <param name="data">CardDetail_SO</param>
    public void AddCard(CardDetail_SO data)
    {
        // Check the cardType of detail to change the card baclgroud image
        Sprite cardBG = GameManager.Instance.CardTypeToCardBackgroud(data);

        // Instantiate the card prefabs
        var cardObj = Instantiate(cardPrefabs, cardInstPoint.transform.position, Quaternion.identity, this.transform) as GameObject;

        // Set new object var
        cardObj.name = $"Card{instCardNameNum}";
        cardObj.GetComponent<BasicCard>().CardInit(data, cardBG);
        instCardNameNum++;
    }


    public void AddCardButton()
    {
        // Random the cardDetail
        CardDetail_SO cardDetail = Cards.Cards[Random.Range(0, Cards.Cards.Count)];

        AddCard(cardDetail);
    }
}
