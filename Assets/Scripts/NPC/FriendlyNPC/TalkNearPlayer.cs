    using System.Collections;
    using NUnit.Framework;
    using TMPro;
    using UnityEngine;

    public class TalkNearPlayer : MonoBehaviour
    {
        [SerializeField] NPCDialogue dialogueData;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] TMP_Text dialogueText;
        [SerializeField] TMP_Text npcNameText;
        private bool isTyping;
        private int dialogueIndex;
        private bool isDialogueActive;
        public void PlayerInRange()
        {
            dialoguePanel = UIManager.Instance.dialoguePanel;
            dialoguePanel.TryGetComponent<SimpleDialogue>(out var simpleDialogue);
            dialogueText = simpleDialogue.dialogueText;
            npcNameText = simpleDialogue.npcNameText;
            StartDialogue();
        }
        public void PlayerOutOfRange()
        {
            StopAllCoroutines();
            isDialogueActive= false;
            dialogueText.SetText("");
            dialoguePanel.SetActive(false);
        }
        void StartDialogue()
        {
            isDialogueActive = true;
            dialogueIndex = 0;

            npcNameText.text = dialogueData.npcName;
            dialoguePanel.SetActive(true);
            
            StartCoroutine(TypeLine());
        }
        IEnumerator TypeLine()
        {
            isTyping = true;
            dialogueText.SetText("");
            foreach(char letter in dialogueData.dialogueLine[dialogueIndex])
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(dialogueData.textSpeed);
            }
            //finish show letter
            isTyping = false;
            
            yield return new WaitForSeconds(dialogueData.autoProgressDeley);
            //display nextline
            NextLine();
        }
        void NextLine()
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(dialogueData.dialogueLine[dialogueIndex]);
                isTyping =false;
                return;
            }
            if (dialogueIndex < dialogueData.dialogueLine.Length - 1)
            {
                dialogueIndex++;
                StartCoroutine(TypeLine());
            }
            else
            {
                //hold last line
            }
        }
        void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag("Player"))
            PlayerInRange();
        }
        void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.CompareTag("Player"))
            PlayerOutOfRange();
        }
    }
