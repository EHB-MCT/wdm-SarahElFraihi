using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// --- DATA STRUCTURES ---
[System.Serializable]
public class StoryNode {
    [TextArea(3, 10)] public string dialogue; // What the boss says
    public string expression;                 // "Neutral", "Angry", "Smiling"
    public string traitTarget;                // For backend: "Neuroticism", "Agreeableness", etc.
    public Choice[] choices;
}

[System.Serializable]
public class Choice {
    public string text;
    public float weight;      // +1 or -1 for the trait
    public int nextNodeIndex; // Where does this choice lead?
}

public class BureaucracyEngine : MonoBehaviour {

    [Header("--- UI LINKS (Drag Objects Here) ---")]
    public Image bossImage;              // The Sprite Object of the cup-head
    public TextMeshProUGUI dialogueText; // The text ON the file folder
    public Button buttonA;
    public Button buttonB;
    public TextMeshProUGUI buttonAText;
    public TextMeshProUGUI buttonBText;
    
    [Header("--- BOSS SPRITES (Drag Sprites Here) ---")]
    public Sprite spriteNeutral; // Drag 'Neutral' sprite here
    public Sprite spriteAngry;   // Drag 'Angry' sprite here
    public Sprite spriteSmiling; // Drag 'Smiling' sprite here

    // --- INTERNAL DATA ---
    private List<StoryNode> story = new List<StoryNode>();
    private int currentNodeIndex = 0;
    private string uid; 
    
    // WMD SPY VARIABLES
    private float startTime;
    private float totalMouseDistance;
    private Vector3 lastMousePosition;

    void Start() {
        // 1. Generate Player ID
        uid = System.Guid.NewGuid().ToString();
        Debug.Log("Subject ID: " + uid);

        // 2. Build the Full Narrative
        BuildStory();

        // 3. Setup Buttons
        buttonA.onClick.AddListener(() => OnChoiceSelected(0));
        buttonB.onClick.AddListener(() => OnChoiceSelected(1));

        // 4. Start Game
        LoadNode(0);
    }

    void Update() {
        // TRACKING MOUSE JITTER (The "Hidden" Metric)
        float distance = Vector3.Distance(Input.mousePosition, lastMousePosition);
        totalMouseDistance += distance;
        lastMousePosition = Input.mousePosition;
    }

    // --- ENGINE LOGIC ---

    void LoadNode(int index) {
        if (index >= story.Count) {
            StartCoroutine(GetFinalVerdict()); // End of game
            return;
        }

        currentNodeIndex = index;
        StoryNode node = story[index];

        // 1. Set Text
        dialogueText.text = node.dialogue;

        // 2. Set Expression
        if (bossImage != null) {
            switch (node.expression) {
                case "Angry": bossImage.sprite = spriteAngry; break;
                case "Smiling": bossImage.sprite = spriteSmiling; break;
                default: bossImage.sprite = spriteNeutral; break;
            }
        }

        // 3. Set Buttons (Hide B if only 1 choice exists)
        if (node.choices.Length > 0) {
            buttonAText.text = node.choices[0].text;
            buttonA.gameObject.SetActive(true);
        } else { buttonA.gameObject.SetActive(false); }

        if (node.choices.Length > 1) {
            buttonBText.text = node.choices[1].text;
            buttonB.gameObject.SetActive(true);
        } else { buttonB.gameObject.SetActive(false); }

        // 4. Reset Spy Metrics
        startTime = Time.time;
        totalMouseDistance = 0f;
        lastMousePosition = Input.mousePosition;
    }

    void OnChoiceSelected(int choiceIndex) {
        StoryNode node = story[currentNodeIndex];
        Choice choice = node.choices[choiceIndex];

        // Capture Data
        float reactionTime = (Time.time - startTime) * 1000; // ms

        // Send to Docker (Fire and forget)
        StartCoroutine(SendData(node.traitTarget, choice.weight, reactionTime, totalMouseDistance));

        // Next Node
        LoadNode(choice.nextNodeIndex);
    }

    // --- THE NARRATIVE (5 Minute Content) ---
    void BuildStory() {
        // NODE 0: INTRO
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "CASE NO. 89-X.\n\nSit down. Don't touch anything. \nWe have been processing your digital footprint for... quite some time.",
            choices = new Choice[] {
                new Choice { text = "Who are you?", nextNodeIndex = 1, weight = 0 },
                new Choice { text = "(Remain Silent)", nextNodeIndex = 1, weight = 0 }
            }
        });

        // NODE 1: INTIMIDATION
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "Oh, names are irrelevant here. I am the Algorithm. You are the Data Point. \n\nLet's verify your 'humanity', shall we? Try not to lie. I can see your heart rate through the webcam.",
            traitTarget = "Neuroticism", // Jitter test
            choices = new Choice[] {
                new Choice { text = "I'm ready.", nextNodeIndex = 2, weight = 1 },
                new Choice { text = "This is illegal.", nextNodeIndex = 2, weight = -1 }
            }
        });

        // NODE 2: AGREEABLENESS TEST
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Scenario 1: \n\nA coworker is crying in the breakroom. They are wasting company time. What is your immediate reaction?",
            traitTarget = "Agreeableness",
            choices = new Choice[] {
                new Choice { text = "Comfort them.", nextNodeIndex = 3, weight = 1 }, // Nice
                new Choice { text = "Report them.", nextNodeIndex = 3, weight = -1 }  // Mean
            }
        });

        // NODE 3: PRESSURE
        story.Add(new StoryNode {
            expression = "Angry",
            dialogue = "Interesting... \n\nYour file says you were late on a payment in 2019. That indicates a lack of discipline. Do you dispute this fact?",
            traitTarget = "Conscientiousness",
            choices = new Choice[] {
                new Choice { text = "It was a mistake!", nextNodeIndex = 4, weight = -1 }, // Defensive
                new Choice { text = "I paid it back.", nextNodeIndex = 4, weight = 1 }   // Responsible
            }
        });

        // NODE 4: OPENNESS TEST
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "We are updating the system protocol. It will require you to relearn your entire job from scratch. \n\nHow does that make you feel?",
            traitTarget = "Openness",
            choices = new Choice[] {
                new Choice { text = "Excited for change.", nextNodeIndex = 5, weight = 1 },
                new Choice { text = "Annoyed.", nextNodeIndex = 5, weight = -1 }
            }
        });

        // NODE 5: THE TRAP (Neuroticism)
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "ALERT. \n\nWe just detected a discrepancy in your file. If you don't click the 'Fix' button in the next 3 seconds, your application is deleted.",
            traitTarget = "Neuroticism", // High jitter expected here
            choices = new Choice[] {
                new Choice { text = "FIX IT NOW!", nextNodeIndex = 6, weight = -1 }, // Panic
                new Choice { text = "Wait, what?", nextNodeIndex = 6, weight = 1 }   // Calm
            }
        });

        // NODE 6: FINAL
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Calibration complete. \n\nProcessing biometrics... \nAnalyzing mouse tremors... \nCalculating social value...",
            choices = new Choice[] {
                new Choice { text = "View Verdict", nextNodeIndex = 99, weight = 0 } // 99 = End
            }
        });
    }

    // --- NETWORKING ---
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
        dialogueText.text = "CONNECTING TO SERVER...";
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        string url = "http://localhost:3000/api/profile/" + uid;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            string json = request.downloadHandler.text;
            
            if (json.Contains("\"verdict\":\"REJECT\"")) {
                bossImage.sprite = spriteAngry;
                dialogueText.text = "<color=red>APPLICATION REJECTED.</color>\n\nReason: Subject exhibits high instability. \nRisk Assessment: CRITICAL. \n\nGet out of my office.";
            } else {
                bossImage.sprite = spriteSmiling;
                dialogueText.text = "<color=green>HIRED.</color>\n\nReason: Compliance levels within acceptable parameters. \n\nWelcome to the machine.";
            }
        } else {
            dialogueText.text = "ERROR: Server offline.\n(Did you run 'docker compose up'?)\n\nAUTO-REJECTED.";
        }
    }
}