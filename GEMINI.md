# AI Agent Directives & Token Optimization Rules

## 1. Token Conservation & Efficiency Rules (CRITICAL)
- **Concise Outputs**: Do NOT write lengthy greetings, conversational fillers, or boilerplate introductions/outros. Jump directly to the action or concise answer.
- **Minimal File Dumps**: Never rewrite an entire file if editing a small portion. Use targeted line replacements with minimal surrounding context lines.
- **Targeted Reading**: Do not view entire 800+ line files if only inspecting a specific class or method. Specify `StartLine` and `EndLine` parameters.
- **Avoid Redundant Re-Reading**: Reuse file content and error logs already present in the conversation context.
- **Brief Summaries**: Keep end-of-turn summaries strictly to 1–3 concise bullet points.
- **No Unnecessary Comments**: Avoid adding verbose XML/doc comments or trivial code comments unless explicitly requested.

## 2. Project Architecture & Context
- **Project Type**: Unity 3D Game (`copyjjk_game`) using Universal Render Pipeline (URP).
- **Primary Source Code**: `Assets/Scripts/` (C# MonoBehaviour and editor scripts).
- **Core Dependencies**: Unity Engine, TextMeshPro, ProBuilder, URP.

## 3. C# & Unity Coding Standards
- Use `PascalCase` for public members, methods, properties, and classes.
- Use `_camelCase` or `camelCase` for private fields (`[SerializeField] private float _speed`).
- Avoid memory allocations in `Update()`, `FixedUpdate()`, or `LateUpdate()`.
- Use `TryGetComponent<T>()` instead of `GetComponent<T>()` where applicable.
- Null-check Unity objects explicitly (`obj != null`).
