
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogue : MonoBehaviour
{

    [SerializeField]
    private GameObject dialogueCanvas;

    [SerializeField]
    private TMP_Text speakerText;

    [SerializeField]
    private TMP_Text dialogueText;

    [SerializeField]
    private Image protraitImage;

    //Dialogue Content
    [SerializeField]
    private string[] speaker;

    [SerializeField]
    [TextArea]
    private string[] dialogueWords;

    [SerializeField]
    private Sprite[] portrait;

    public DialogueChoice[] choices;
    public string nextDialogueID;

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextDialogueID;
    }

    private bool dialogueActivated;
    private int step;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player") { 
             dialogueActivated = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && dialogueActivated == true)
        {
            if (step >= speaker.Length)
            {
                dialogueCanvas.SetActive(false);
                step = 0;

            }
            else {
                dialogueCanvas.SetActive(true);
                speakerText.text = speaker[step];
                dialogueText.text = dialogueWords[step];
                protraitImage.sprite = portrait[step];
                step += 1;
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        dialogueActivated = false;
        dialogueCanvas.SetActive(false);
    }
}
