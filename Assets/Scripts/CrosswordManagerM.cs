using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrosswordManagerM : MonoBehaviour
{

    public static CrosswordManagerM instance;

    public TMP_InputField hiddenInput;

    private CellData currentCell;

    private int count;

    private CrosswordWord currentActiveWord = null;

    private Dictionary<Vector2Int, CellData> gridCells =
        new Dictionary<Vector2Int, CellData>();

    private Dictionary<CellData, List<CrosswordWord>> inputFieldToWords =
        new Dictionary<CellData, List<CrosswordWord>>();

    private Dictionary<CellData, int> inputClickState =
        new Dictionary<CellData, int>();

    [System.Serializable]
    public class CrosswordWord
    {
        public string word;
        public int hintIndex;
        public Vector2Int startPosition;
        public bool isHorizontal;
    }

    public List<CrosswordWord> words;

    public int gridSize;

    public GameObject cellPrefab;

    public Transform gridParent;
    
    private bool hintShown = false;
    private List<(CellData cell, string letter)> hintCells =
        new List<(CellData, string)>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        hiddenInput.characterLimit = 1;

        hiddenInput.shouldHideMobileInput = true;

        hiddenInput.onValueChanged.RemoveAllListeners();
        hiddenInput.onValueChanged.AddListener(TypeLetter);
    }

    void TypeLetter(string value)
    {
        Debug.Log("Typing : " + value);

        if (string.IsNullOrEmpty(value))
            return;

        if (currentCell == null)
            return;

        string letter = value.Substring(value.Length - 1).ToUpper();

        currentCell.SetLetter(letter);

        Debug.Log(currentCell.name);
        Debug.Log(currentCell.letterText.text);

        hiddenInput.onValueChanged.RemoveListener(TypeLetter);

        hiddenInput.text = "";

        hiddenInput.onValueChanged.AddListener(TypeLetter);

        MoveNextCell();

        StartCoroutine(ReopenKeyboard());
    }

    IEnumerator ReopenKeyboard()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(hiddenInput.gameObject);

        hiddenInput.Select();
        hiddenInput.ActivateInputField();
    }

    void MoveNextCell()
    {
        if (currentActiveWord == null || currentCell == null)
            return;

        int currentIndex = -1;

        for (int i = 0; i < currentActiveWord.word.Length; i++)
        {
            Vector2Int pos = currentActiveWord.isHorizontal
                ? new Vector2Int(
                    currentActiveWord.startPosition.x + i,
                    currentActiveWord.startPosition.y)
                : new Vector2Int(
                    currentActiveWord.startPosition.x,
                    currentActiveWord.startPosition.y + i);

            if (pos == currentCell.gridPosition)
            {
                currentIndex = i;
                break;
            }
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= currentActiveWord.word.Length)
            return;

        Vector2Int nextPos = currentActiveWord.isHorizontal
            ? new Vector2Int(
                currentActiveWord.startPosition.x + nextIndex,
                currentActiveWord.startPosition.y)
            : new Vector2Int(
                currentActiveWord.startPosition.x,
                currentActiveWord.startPosition.y + nextIndex);

        if (gridCells.ContainsKey(nextPos))
        {
            //currentCell.NormalColor(); 
            currentCell.StopBlink();


            currentCell = gridCells[nextPos];

            currentCell.SelectCellColor();

            hiddenInput.ActivateInputField();
        }
    }

    public void StartMediumlevel()
    {
        count = 0;

        hiddenInput.onValueChanged.RemoveAllListeners();
        hiddenInput.onValueChanged.AddListener(TypeLetter);

        GameManager.instance.MediumLevel();

        GameManager.instance.timerRunning = true;

        GenerateGrid();
    }

    public void ClearGrid()
    {
        foreach (Transform child in gridParent.transform)
        {
            Destroy(child.gameObject);
        }

        gridCells.Clear();

        inputFieldToWords.Clear();

        currentActiveWord = null;
    }

    void GenerateGrid()
    {
        ClearGrid();

        int wordIndex = 1;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject cellObj =
                    Instantiate(cellPrefab, gridParent);

                CellData cell =
                    cellObj.GetComponent<CellData>();

                Vector2Int pos = new Vector2Int(x, y);

                cell.Setup(pos);

                cell.DisableCell();

                gridCells[pos] = cell;
            }
        }

        for (int w = 0; w < words.Count; w++)
        {
            CrosswordWord word = words[w];

            for (int i = 0; i < word.word.Length; i++)
            {
                Vector2Int position = word.isHorizontal
                    ? new Vector2Int(
                        word.startPosition.x + i,
                        word.startPosition.y)
                    : new Vector2Int(
                        word.startPosition.x,
                        word.startPosition.y + i);

                if (gridCells.ContainsKey(position))
                {
                    CellData cell = gridCells[position];

                    cell.EnableCell();

                    //cell.currentWord = null;

                    if (!inputFieldToWords.ContainsKey(cell))
                        inputFieldToWords[cell] =
                            new List<CrosswordWord>();

                    inputFieldToWords[cell].Add(word);

                    if (i == 0)
                    {
                        cell.SetNumber(wordIndex.ToString());

                        wordIndex++;
                    }
                }
            }
        }
    }

    public void CheckAnswers()
    {
        bool allCorrect = true;
    bool hasAnyLetter = false;
    bool hasAnyCorrectWord = false;

    count = 0;

    foreach (var word in words)
    {
        string playerAnswer = "";

        List<CellData> letterFields = new List<CellData>();

        for (int i = 0; i < word.word.Length; i++)
        {
            Vector2Int position =
                word.isHorizontal
                    ? new Vector2Int(
                        word.startPosition.x + i,
                        word.startPosition.y)
                    : new Vector2Int(
                        word.startPosition.x,
                        word.startPosition.y + i);

            if (gridCells.ContainsKey(position))
            {
                CellData inputField = gridCells[position];

                playerAnswer += inputField.letterText.text.ToUpper();

                // Check if at least one letter is entered
                if (!string.IsNullOrEmpty(inputField.letterText.text))
                {
                    hasAnyLetter = true;
                }

                letterFields.Add(inputField);
            }
        }

        if (playerAnswer == "")
        {
            allCorrect = false;
        }
        else
        {
            if (playerAnswer == word.word.ToUpper())
            {
                hasAnyCorrectWord = true;
                count++;

                foreach (CellData inputField in letterFields)
                {
                    inputField.CorrectColor();
                }
            }
            else
            {
                allCorrect = false;
            }
        }
    }

    // Puzzle empty
    if (!hasAnyLetter)
    {
        GameManager.instance.tryAgainText.text = "Puzzle Incomplete";
        GameManager.instance.EnableTryagain();
        return;
    }

    // No correct words
    if (!hasAnyCorrectWord)
    {
        GameManager.instance.tryAgainText.text = "Answers Are Wrong";
        GameManager.instance.EnableTryagain();
        return;
    }

    // Some correct, some wrong
    if (!allCorrect)
    {
        GameManager.instance.tryAgainText.text =
            "Some Answers Are Incorrect. Try Again!";
        GameManager.instance.EnableTryagain();
        return;
    }

    // All correct
    if (count >= words.Count && allCorrect)
    {
        GameManager.instance.EnableEasyWinpanel();
        GameManager.instance.timerRunning = false;
        GameManager.instance.playagainbtn.SetActive(true);
    }
       
    }



    public void ShowHint()
    {
        // First click -> Show Hint
        if (!hintShown)
        {
            hintCells.Clear();

            foreach (var word in words)
            {
                if (word.hintIndex < 0 || word.hintIndex >= word.word.Length)
                    continue;

                Vector2Int position = word.isHorizontal
                    ? new Vector2Int(
                        word.startPosition.x + word.hintIndex,
                        word.startPosition.y)
                    : new Vector2Int(
                        word.startPosition.x,
                        word.startPosition.y + word.hintIndex);

                if (gridCells.ContainsKey(position))
                {
                    CellData cell = gridCells[position];

                    string correctLetter =
                        word.word[word.hintIndex].ToString();

                    cell.SetLetter(correctLetter);

                    hintCells.Add((cell, correctLetter));
                }
            }

            hintShown = true;
        }
        // Second click -> Clear Hint
        else
        {
            foreach (var hintData in hintCells)
            {
                // Clear only hint letters
                if (hintData.cell.letterText.text ==
                    hintData.letter)
                {
                    hintData.cell.SetLetter("");
                }
            }

            hintCells.Clear();
            hintShown = false;
        }
    }
    
    private void HighlightWord(CrosswordWord wordToHighlight)
    {
        foreach (var kvp in gridCells)
        {
            if (kvp.Value.GetComponent<Button>().interactable)
            {
                kvp.Value.bgImage.color = Color.white;
            }
        }

        for (int i = 0; i < wordToHighlight.word.Length; i++)
        {
            Vector2Int pos = wordToHighlight.isHorizontal
                ? new Vector2Int(
                    wordToHighlight.startPosition.x + i,
                    wordToHighlight.startPosition.y)
                : new Vector2Int(
                    wordToHighlight.startPosition.x,
                    wordToHighlight.startPosition.y + i);

            if (gridCells.ContainsKey(pos))
            {
                gridCells[pos].bgImage.color =
                    new Color(1f, 0.92f, 0.6f);
            }
        }
    }
    public void SelectCell(CellData cell)
    {
        
        // Stop blinking on previously selected cell
        if (currentCell != null)
        {
            currentCell.StopBlink();
            currentCell.NormalColor();
        }

        currentCell = cell;

        List<CrosswordWord> relatedWords =
            inputFieldToWords[currentCell];

        if (relatedWords.Count == 1)
        {
            currentActiveWord = relatedWords[0];
        }
        else
        {
            if (!inputClickState.ContainsKey(currentCell))
                inputClickState[currentCell] = 0;

            int toggleIndex = inputClickState[currentCell];

            currentActiveWord = relatedWords[toggleIndex];

            inputClickState[currentCell] = 1 - toggleIndex;
        }

        HighlightWord(currentActiveWord);

        currentCell.SelectCellColor();

        hiddenInput.text = "";

        StartCoroutine(OpenKeyboard());
    }

    IEnumerator OpenKeyboard()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(hiddenInput.gameObject);

        hiddenInput.Select();

        hiddenInput.ActivateInputField();

#if UNITY_ANDROID
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
#endif
    }
    
    public void ClearAllWords()
    {
        hintShown = false;
        foreach (var kvp in gridCells)
        {
            CellData cell = kvp.Value;

            if (cell.GetComponent<Button>().interactable)
            {
                // Clear letter
                cell.SetLetter("");

                // Stop blink animation
                cell.StopBlink();

                // Reset color
                cell.NormalColor();
            }
        }

        // Clear current selection
        currentCell = null;
        currentActiveWord = null;

        hiddenInput.text = "";
    }
    public void Clearallcells()
    {
        hintShown = false;
        hintCells.Clear();
       
    }
}