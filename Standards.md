# Unity C# Naming and Code Style Guide

Based on: [Unity Naming and Code Style Tips — C# Scripting](https://unity.com/how-to/naming-and-code-style-tips-c-scripting#identifier-names)

## Identifier Names

### General

- Use **PascalCase** for most names (each word capitalized, no underscores).
- Be descriptive but concise — names should clearly describe what the variable, method, or class does.
- Avoid abbreviations unless they are well-known (like `UI`, `ID`, `RGB`).
- Keep names consistent across scripts and assets.

### Classes, Structs, Enums, Interfaces

- Use **PascalCase**.
- Class names should be **nouns** that describe what they represent.
- Interfaces start with a capital **I**.

**Examples:**

```csharp
public class PlayerController { }
public struct DamageData { }
public enum GameState { Playing, Paused, Ended }
public interface IInteractable { }
```

### Methods, Properties, and Events

- Use **PascalCase** for all public methods and properties.
- Method names should be **verbs** that describe actions.
- Use an **"On"** prefix for events.

**Examples:**

```csharp
void StartGame();
int GetScore();
public event Action OnPlayerDied;
```

### Fields and Variables

- **Private fields:** `_camelCase` (with a leading underscore).
- **Public fields:** `PascalCase`.
- **Local variables and parameters:** `camelCase`.

**Examples:**

```csharp
[SerializeField] private float _moveSpeed;
public int Health;
void MovePlayer(Vector3 direction) { }
```

### Constants and Static Readonly Fields

- Use **PascalCase** and make them clear and descriptive.

**Example:**

```csharp
public const float MaxSpeed = 10f;
public static readonly Color DefaultColor = Color.white;
```

### Acronyms and Abbreviations

- Only keep acronyms fully uppercase if they are short and common (e.g. `UI`, `ID`, `XML`).
- Otherwise, treat them as normal words (`HttpRequest`, not `HTTPRequest`).

## File and Script Naming

- The **file name must match the class name** inside it.
- One public class or interface per file.
- Use **namespaces** to group related code (e.g. `MyGame.UI`).

**Example:**

File: PlayerController.cs
Contains: public class PlayerController { }

## Formatting and Layout

- Use **4 spaces** for indentation (no tabs).
- **Braces** go on **new lines** (Allman style).
- Keep code blocks small and organized by functionality.
- Leave a blank line between methods.
- Limit lines

## Member Order

Recommended order inside a class:

1. Constants / Static Fields
2. Serialized Private Fields
3. Non-Serialized Fields
4. Properties
5. Events
6. Unity Lifecycle Methods (`Awake`, `Start`, `Update`, etc.)
7. Public Methods
8. Private Helper Methods

## Serialization and Inspector Usage

- Prefer `[SerializeField] private` over public fields.
- Use attributes like `[Tooltip]`, `[Range]`, `[Header]` to make inspectors clearer.
- Use `[RequireComponent(typeof(...))]` to enforce dependencies.
- Avoid `[HideInInspector]` unless absolutely necessary.

## Asset and Scene Naming

- **Prefabs:** `Enemy_Goblin`, `Weapon_Sword_Iron`
- **Scenes:** `MainMenu`, `Level01`
- **Materials:** `Mat_Player_Body`
- **Animations:** `Anim_Run`, `Anim_Idle`
