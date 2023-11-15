using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;


public class LevelController : MonoBehaviour
{
    [Serializable]
    public class LevelData
    {
        public int Columns;
        public int Rows;
        public int Difficulty;
        public int Movements;
    }

    [SerializeField]
    private CardController _cardPrefab;

    [Header("UI references")]
    [SerializeField]
    private TMP_Text _levelText;
    [SerializeField]
    private TMP_Text _movementsText;
    [SerializeField]
    private GameObject _gameOverButton;

    [Header("LevelData")]
    [SerializeField]
    private List<LevelData> _levels = new List<LevelData>();
    

    private List<CardController> _cards = new List<CardController>();
    private CardController _activeCard;
    private int _movementsUsed = 0;
    private bool _blockInput = true;
    private int _level = 0;

    private void Start()
    {
        _level = PlayerPrefs.GetInt("Level", 0);
        StartLevel();
    }

    public void StartLevel()
    {
        _gameOverButton.SetActive(false);

        if (_levels[_level] * Difficulty > _cardPrefab.MaxCardTypes)
        {
            Debug.Assert(false);
            _levels[_level] * Difficulty = Math.Min(_levels[_level] * Difficulty, _cardPrefab.MaxCardTypes);
        }
        Debug.Assert((_levels[_level]*Rows * _levels[_level] * Columns) % 2 == 0);

        _cards.ForEach(c => Destroy(c.gameObject));
        _cards.Clear();

        List<int> allTypes = new List<int>();
        for (int i = 0; i < _cardPrefab.MaxCardTypes; i++)
        {
            allTypes.Add(i);
        }
        List<int> gameTypes = new List<int>();
        for (int i = 0; i < _levels[_level] * Difficulty; i++)
        {
            int chosenType = allTypes[UnityEngine.Random.Range(0, allTypes.Count)];
            allTypes.Remove(chosenType);
            gameTypes.Add(chosenType);
        }
        List<int> chosenTypes = new List<int>();
        for (int i = 0; i < (_levels[_level] * Rows * _levels[_level] * Columns) / 2; i++)
        {
            int chosenType = gameTypes[UnityEngine.Random.Range(0, gameTypes.Count)];
            chosenTypes.Add(chosenType);
            chosenTypes.Add(chosenType);
        }

        Vector3 offset = new Vector3(_levels[_level] * Columns - 1 * _cardPrefab.CardSize, (_levels[_level] * Rows - 1) * _cardPrefab.CardSize, 0f) * 0.5f;
        for (int y = 0; y < _levels[_level] * Rows; y++)
        {
            for (int x = 0; x < _levels[_level] * Columns; ++x)
            {
                Vector3 position = new Vector3(x * _cardPrefab.CardSize, y * _cardPrefab.CardSize, 0f);
                var card = Instantiate(_cardPrefab, position - offset, quaternion.identity);
                card.CardType = chosenTypes[UnityEngine.Random.Range(0, chosenTypes.Count)];
                chosenTypes.Remove(card.CardType);
                card.OnClicked.AddListener(OnCardClicked);
                _cards.Add(card);
            }
        }

        _blockInput = false;
        _movementsUsed = 0;
        _levelText.text = $"Level: {_level}";
        _movementsText.text = $"Moves: {_levels[_level] * Movements}";
    }

    private void OnCardClicked(CardController card)
    {
        if (_blockInput)
        {
            return;
        }

        _blockInput = true;

        if (_activeCard == null)
        {
            StartCoroutine(SelectCard(card));
            return;
        }

        _movementsUsed++;
        _movementsText.text = $"Moves: {_levels[_level] * Movements - _movementsUsed}";


        if (card.CardType == _activeCard.CardType)
        {
            StartCoroutine(Score(card));
            return;
        }
        StartCoroutine(Fail(card));
    }

    private IEnumerator SelectCard(CardController card)
    {
        _activeCard = card;
        _activeCard.Reveal();
        yield return new WaitForSeconds(0.5f);
        _blockInput = false;
    }

    private IEnumerator Score(CardController card)
    {
        card.Reveal();
        yield return new WaitForSeconds(1f);
        _cards.Remove(_activeCard);
        _cards.Remove(card);
        Destroy(card.gameObject);
        Destroy(_activeCard.gameObject);
        _activeCard = null;
        if (_cards.Count < 1)
        {
            Win();
            yield break;
        }
        if (_movementsUsed >= _levels[_level] * Movements)
        {
            Lose();
            yield break;
        }
        _blockInput = false;
    }

    private IEnumerator Fail(CardController card)
    {
        card.Reveal();
        yield return new WaitForSeconds(1f);
        _activeCard.Hide();
        card.Hide();
        _activeCard = null;
        yield return new WaitForSeconds(0.5f);
        if (_movementsUsed >= _levels[_level] * Movements)
        {
            Lose();
            yield break;
        }
        _blockInput = false;
    }

    private void Win()
    {
        _level++;
        if (_level > _levels.Count)
        {
            _level = 0;
        }
        PlayerPrefs.SetInt("Level", _level);
        Debug.Log("Victory");
        _gameOverButton.SetActive(true);

    }

    private void Lose()
    {
        Debug.Log("Defeat");
        _gameOverButton.SetActive(true);

    }

}
