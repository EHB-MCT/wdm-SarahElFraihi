using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class StoryNode {
    [TextArea(3, 10)] public string dialogue; 
    public string expression;                 
    public string traitTarget;                
    public Choice[] choices;
}

[System.Serializable]
public class Choice {
    public string text;
    public float weight;      
    public int nextNodeIndex; 
}

public class BureaucracyEngine : MonoBehaviour {

    [Header("--- UI LINKS ---")]
    public GameObject dimOverlay;
    public Image bossImage;              
    public TextMeshProUGUI dialogueText; 
    
    [Header("--- BUTTONS ---")]
    public Button nextButton;
    public Button buttonA;
    public Button buttonB;
    public TextMeshProUGUI buttonAText;
    public TextMeshProUGUI buttonBText;
    
    [Header("--- BOSS SPRITES ---")]
    public Sprite spriteNeutral; 
    public Sprite spriteAngry;   
    public Sprite spriteSmiling; 

    // --- INTERNAL DATA ---
    private List<StoryNode> story = new List<StoryNode>();
    private int currentNodeIndex = 0;
    private string uid; 
    
    private float startTime;
    private float totalMouseDistance;
    private Vector3 lastMousePosition;

    void Start() {
        uid = System.Guid.NewGuid().ToString();
        
        if (dimOverlay != null) dimOverlay.SetActive(false);

        BuildStory();

        buttonA.onClick.AddListener(() => OnChoiceSelected(0));
        buttonB.onClick.AddListener(() => OnChoiceSelected(1));
        nextButton.onClick.AddListener(OnNextClicked);

        LoadNode(0);
    }

    void Update() {
        float distance = Vector3.Distance(Input.mousePosition, lastMousePosition);
        totalMouseDistance += distance;
        lastMousePosition = Input.mousePosition;
    }

    // --- ENGINE LOGIC ---

    void LoadNode(int index) {
        if (index >= story.Count) {
            StartCoroutine(GetFinalVerdict());
            return;
        }

        currentNodeIndex = index;
        StoryNode node = story[index];

        dialogueText.text = node.dialogue;

        if (bossImage != null) {
            switch (node.expression) {
                case "Angry": bossImage.sprite = spriteAngry; break;
                case "Smiling": bossImage.sprite = spriteSmiling; break;
                default: bossImage.sprite = spriteNeutral; break;
            }
        }

        // MODE LECTURE (Pas de choix, juste Next)
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
        
        nextButton.gameObject.SetActive(true);

        startTime = Time.time;
        totalMouseDistance = 0f;
        lastMousePosition = Input.mousePosition;
    }

    void OnNextClicked() {
        StoryNode node = story[currentNodeIndex];

        nextButton.gameObject.SetActive(false);
        
        // MODE CHOIX (Fond noir + Boutons)
        if (dimOverlay != null) dimOverlay.SetActive(true);

        if (node.choices.Length > 0) {
            buttonAText.text = node.choices[0].text;
            buttonA.gameObject.SetActive(true);
        }
        if (node.choices.Length > 1) {
            buttonBText.text = node.choices[1].text;
            buttonB.gameObject.SetActive(true);
        }
    }

    void OnChoiceSelected(int choiceIndex) {
        StoryNode node = story[currentNodeIndex];
        Choice choice = node.choices[choiceIndex];

        float reactionTime = (Time.time - startTime) * 1000; 
        StartCoroutine(SendData(node.traitTarget, choice.weight, reactionTime, totalMouseDistance));

        LoadNode(choice.nextNodeIndex);
    }

    // --- L'HISTOIRE (Modifiée selon tes demandes) ---
    void BuildStory() {
        // NODE 0: INTRO
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Sit down. Don't touch anything. We have been processing your digital footprint for quite some time.", // Texte plus compact
            choices = new Choice[] {
                new Choice { text = "Who are you?", nextNodeIndex = 1, weight = 0 },
                new Choice { text = "(Remain Silent)", nextNodeIndex = 1, weight = 0 }
            }
        });

        // NODE 1: INTIMIDATION
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "Names are irrelevant. I am the Algorithm. You are the Data Point. Let's verify your 'humanity'. Try not to lie.",
            traitTarget = "Neuroticism", 
            choices = new Choice[] {
                new Choice { text = "I'm ready.", nextNodeIndex = 2, weight = 1 },
                new Choice { text = "This is illegal.", nextNodeIndex = 2, weight = -1 }
            }
        });

        // NODE 2: AGREEABLENESS (Sans "Scenario 1")
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "A coworker is crying in the breakroom. They are wasting company time. What is your immediate reaction?",
            traitTarget = "Agreeableness",
            choices = new Choice[] {
                new Choice { text = "Comfort them.", nextNodeIndex = 3, weight = 1 }, 
                new Choice { text = "Report them.", nextNodeIndex = 3, weight = -1 }  
            }
        });

        // NODE 3: PRESSURE
        story.Add(new StoryNode {
            expression = "Angry",
            dialogue = "Interesting... Your file says you were late on a payment in 2019. That indicates a lack of discipline. Do you dispute this?",
            traitTarget = "Conscientiousness",
            choices = new Choice[] {
                new Choice { text = "It was a mistake!", nextNodeIndex = 4, weight = -1 }, 
                new Choice { text = "I paid it back.", nextNodeIndex = 4, weight = 1 }   
            }
        });

        // NODE 4: OPENNESS
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "We are updating the system protocol. It will require you to relearn your entire job from scratch. How does that make you feel?",
            traitTarget = "Openness",
            choices = new Choice[] {
                new Choice { text = "Excited.", nextNodeIndex = 5, weight = 1 },
                new Choice { text = "Annoyed.", nextNodeIndex = 5, weight = -1 }
            }
        });

        // NODE 5: THE TRAP (Sans "ALERT")
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "We detected a discrepancy. If you don't click 'Fix' in the next 3 seconds, your application is deleted.",
            traitTarget = "Neuroticism", 
            choices = new Choice[] {
                new Choice { text = "FIX IT NOW!", nextNodeIndex = 6, weight = -1 }, 
                new Choice { text = "Wait, what?", nextNodeIndex = 6, weight = 1 }   
            }
        });

        // NODE 6: PRE-ENDING
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Processing complete. Please wait for the final decision.",
            choices = new Choice[] {
                new Choice { text = "View Result", nextNodeIndex = 99, weight = 0 } 
            }
        });
    }

    IEnumerator SendData(string trait, float weight, float time, float dist) {
        string url = "http://localhost:3000/api/track";
        string json = string.Format(
            "{{\"uid\":\"{0}\", \"questionId\":\"STORY\", \"choiceWeight\":{1}, \"traitTarget\":\"{2}\", \"responseTimeMs\":{3}, \"mouseDistance\":{4}}}",
            uid, weight.ToString("F1").Replace(',', '.'), trait, (int)time, (int)dist
        );
        
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
    }

    IEnumerator GetFinalVerdict() {
        dialogueText.text = "CONNECTING...";
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false); // On enlève le noir pour la fin

        yield return new WaitForSeconds(1.5f);

        string url = "http://localhost:3000/api/profile/" + uid;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            string json = request.downloadHandler.text;
            
            // --- FIN PERSONNALISÉE ---
            if (json.Contains("\"verdict\":\"REJECT\"")) {
                bossImage.sprite = spriteAngry;
                // Fin froide et directe
                dialogueText.text = "<color=red>LEAVE.</color> You are not fit for this purpose. Security has been alerted.";
            } else {
                bossImage.sprite = spriteSmiling;
                // Bienvenue corporate
                dialogueText.text = "<color=green>YOU START MONDAY.</color> Do not be late. Welcome to the company.";
            }
        } else {
            dialogueText.text = "ERROR: Server offline.";
        }
    }
}