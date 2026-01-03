using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// --- STRUCTURES DE DONNÉES ---
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

    void BuildStory() {
        
        // 0. INTRO
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Sit down. Do not adjust your chair. We have already processed 89% of your digital life. We just need to verify the remaining 11%.",
            choices = new Choice[] {
                new Choice { text = "Understood.", nextNodeIndex = 1, weight = 1 },
                new Choice { text = "Is this necessary?", nextNodeIndex = 1, weight = -1 }
            }
        });

        // 1. LOYALTY (Agreeableness)
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "We value loyalty above all else. If you discovered your supervisor was making a mistake that cost the company money, would you report them immediately?",
            traitTarget = "Agreeableness", 
            choices = new Choice[] {
                new Choice { text = "Yes, absolutely.", nextNodeIndex = 2, weight = 1 }, // Loyal au système
                new Choice { text = "No, I'd talk to them.", nextNodeIndex = 2, weight = -1 } // Humain (Mauvais pour l'algo)
            }
        });

        // 2. WORK ETHIC (Conscientiousness)
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "The weekend is a social construct. Our algorithm predicts a surge in workload next Sunday. Are you available?",
            traitTarget = "Conscientiousness",
            choices = new Choice[] {
                new Choice { text = "I am available.", nextNodeIndex = 3, weight = 1 }, 
                new Choice { text = "I have plans.", nextNodeIndex = 3, weight = -1 }   
            }
        });

        // 3. PRESSURE TEST (Neuroticism - Jitter)
        story.Add(new StoryNode {
            expression = "Angry",
            dialogue = "Don't lie to me. My sensors indicate a spike in your cortisol levels. Why are you nervous? You have nothing to hide, do you?",
            traitTarget = "Neuroticism",
            choices = new Choice[] {
                new Choice { text = "I'm not nervous.", nextNodeIndex = 4, weight = 1 }, 
                new Choice { text = "Sorry...", nextNodeIndex = 4, weight = -1 }
            }
        });

        // 4. PRIVACY (Openness)
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "To optimize your workflow, we need access to your private social media messages. It's for 'Safety Monitoring'. Do you consent?",
            traitTarget = "Openness",
            choices = new Choice[] {
                new Choice { text = "I Consent.", nextNodeIndex = 5, weight = 1 }, // Soumis
                new Choice { text = "That's private!", nextNodeIndex = 5, weight = -1 } // Rebelle
            }
        });

        // 5. UNION / POLITICS (Agreeableness - Piège)
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Hypothetical question: A group of employees is forming a 'Union' to demand higher wages. Do you join them?",
            traitTarget = "Agreeableness",
            choices = new Choice[] {
                new Choice { text = "I join them.", nextNodeIndex = 6, weight = -5 }, // GROS MALUS (Rebelle)
                new Choice { text = "I report them.", nextNodeIndex = 6, weight = 5 } // GROS BONUS (Drone)
            }
        });

        // 6. FAMILY (Conscientiousness)
        story.Add(new StoryNode {
            expression = "Neutral",
            dialogue = "Your mother is sick. The company has a deadline the same day. Where are you?",
            traitTarget = "Conscientiousness",
            choices = new Choice[] {
                new Choice { text = "At work.", nextNodeIndex = 7, weight = 2 }, 
                new Choice { text = "With family.", nextNodeIndex = 7, weight = -2 }
            }
        });

        // 7. THE TRAP (Neuroticism - Speed check)
        story.Add(new StoryNode {
            expression = "Angry",
            dialogue = "EMERGENCY ALERT! Data breach detected! Confirm your identity NOW! You have 3 seconds!",
            traitTarget = "Neuroticism", 
            choices = new Choice[] {
                new Choice { text = "CONFIRM!", nextNodeIndex = 8, weight = 0 }, 
                new Choice { text = "CONFIRM!", nextNodeIndex = 8, weight = 0 }   
            }
        });

        // 8. FINAL QUESTION
        story.Add(new StoryNode {
            expression = "Smiling",
            dialogue = "One last thing. Do you believe an Algorithm can make better decisions than a human?",
            traitTarget = "Openness",
            choices = new Choice[] {
                new Choice { text = "Yes, algorithms are pure.", nextNodeIndex = 99, weight = 5 }, 
                new Choice { text = "No, humans have soul.", nextNodeIndex = 99, weight = -5 }
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
        dialogueText.text = "CALCULATING SOCIAL SCORE...";
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);

        yield return new WaitForSeconds(2.0f);

        string url = "http://localhost:3000/api/profile/" + uid;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            string json = request.downloadHandler.text;
            Debug.Log(json); 
            
            
            bool isRejected = json.Contains("\"verdict\":\"REJECT\"");
            
            
            if (isRejected) {
                bossImage.sprite = spriteAngry;
                
                if (json.Contains("\"neuroticism\":8") || json.Contains("\"neuroticism\":9") || json.Contains("\"neuroticism\":100")) {
                     dialogueText.text = "<color=red>REJECTED: UNSTABLE.</color>Your mouse movements betray severe anxiety. We need confidence, not hesitation.Get out.";
                }
                else {
                     dialogueText.text = "<color=red>REJECTED: INSUBORDINATE.</color>You prioritize 'ethics' and 'family' over efficiency. The Algorithm detects a potential unionizer.Security will escort you out.";
                }
            } 
            else {
                bossImage.sprite = spriteSmiling;
                dialogueText.text = "<color=green>APPLICATION ACCEPTED.</color>You are obedient, predictable, and devoid of unnecessary morals.Welcome to the family. You start immediately.";
            }
        } else {
            dialogueText.text = "ERROR: Server Connection Failed.";
        }
    }
}