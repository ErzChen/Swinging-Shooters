using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)] public string lineText;
}

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI speaker;
    public ExpandOnStart panel;
    public DialogueLine[] dialogues;

    [SerializeField] private float typingSpeed = 0.05f;

    private Coroutine dialogueCoroutine;
    private static Dialogue currentDialogue;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        speaker = transform.parent.Find("Speaker").GetComponent<TextMeshProUGUI>();
        panel = transform.parent.GetComponent<ExpandOnStart>();

        BeginDialogue();
    }

    private void BeginDialogue()
    {
        if (currentDialogue != null && currentDialogue != this)
        {
            currentDialogue.CancelDialogue();
        }

        currentDialogue = this;
        dialogueCoroutine = StartCoroutine(PlayDialogue());
    }
    public void CancelDialogue()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        text.text = "";
        speaker.text = "";

        if (panel != null)
        {
            panel.StopCoroutine("ShrinkOverTime"); 
            panel.StartCoroutine("ShrinkOverTime");
        }
    }

    IEnumerator PlayDialogue()
    {
        foreach (DialogueLine dialogue in dialogues)
        {
            speaker.text = dialogue.speakerName;
            string line = dialogue.lineText;
            text.text = "";

            foreach (char letter in line.ToCharArray())
            {
                while (Time.timeScale == 0f)
                {
                    yield return null;
                }
                text.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(3f);
        }

        text.text = "";
        panel.StartCoroutine("ShrinkOverTime");

        if (currentDialogue == this)
        {
            currentDialogue = null;
        }
    }
}