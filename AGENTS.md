# AI Agent Directives & Token Optimization Rules

## 1. Token Conservation & Efficiency Rules (CRITICAL)
- **Concise Outputs**: Do NOT write lengthy greetings, conversational fillers, or boilerplate introductions/outros (e.g., "Sure, I can help with that!", "Here is the code you requested:"). Jump directly to the action or concise answer.
- **Minimal File Dumps**: Never rewrite an entire file if editing a small portion. Use targeted line replacements (`replace_file_content` / `multi_replace_file_content`) with minimal surrounding context lines.
- **Targeted Reading**: Do not view entire 800+ line files if only inspecting a specific class or method. Specify `StartLine` and `EndLine` parameters in `view_file` calls.
- **Avoid Redundant Re-Reading**: Reuse file content and error logs already present in the conversation context. Do not call `view_file` or `run_command` repeatedly on unchanged resources.
- **Brief Summaries**: Keep end-of-turn summaries strictly to 1–3 concise bullet points. Avoid repeating code changes or diffs in the text response when they were already written to files.
- **No Unnecessary Comments**: Avoid adding verbose XML/doc comments or trivial code comments unless explicitly requested.

## 2. Project Architecture & Context
- **Project Type**: Unity 3D Game (`copyjjk_game`) using Universal Render Pipeline (URP).
- **Primary Source Code**: `Assets/Scripts/` (C# MonoBehaviour and editor scripts).
- **Core Dependencies**: Unity Engine, TextMeshPro, ProBuilder, URP.

## 3. C# & Unity Coding Standards
- Use `PascalCase` for public members, methods, properties, and classes.
- Use `_camelCase` or `camelCase` for private fields (`[SerializeField] private float _speed`).
- Avoid memory allocations in `Update()`, `FixedUpdate()`, or `LateUpdate()` (e.g., avoid `new`, LINQ, or `GetComponent<T>()` in loop/frame updates).
- Use `TryGetComponent<T>()` instead of `GetComponent<T>()` where applicable.
- Null-check Unity objects explicitly (`obj != null`) rather than using null-coalescing (`??`) due to Unity's custom equality operator.

## 4. Workflows & Verification
- Verify C# script syntax and standard compilation cleanly when editing scripts.
- Keep modifications focused strictly on the requested feature or fix. Do not refactor unrelated code.
