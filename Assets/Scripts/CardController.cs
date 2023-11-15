using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class CardController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> prefabs;

    public int MaxCardTypes => prefabs.Count;

    public float CardSize = 2f;

    public UnityEvent<CardController> OnClicked;

    public int CardType = -1;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Start()
    {
        if (CardType <0)
        {
            CardType = UnityEngine.Random.Range(0, prefabs.Count);
        }

        Instantiate(prefabs[CardType],transform.position,quaternion.identity, transform);
    }

    private void OnMouseUpAsButton()
    {
        OnClicked.Invoke(this);
    }

    public void TestAnimation()
    {
        IEnumerator AnimationCoroutine()
        {
            Reveal();
            yield return new WaitForSeconds(2);
            Hide();
        }

        StartCoroutine(AnimationCoroutine());
    }

}
