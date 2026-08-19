Moroccan Blackjack — Dev Log
Objective

Building a small horror/atmosphere Blackjack game in Unity (C#), themed around a first-person, single-room encounter with a mysterious Moroccan dealer. Inspired by the tension/presentation of games like Buckshot Roulette — simple mechanics, strong atmosphere. Full design spec lives in the project's design document; this file is a living summary of where the project currently stands.

Primary personal goal: the developer (Yassir) is using this project to learn C# and Unity from scratch, not just to get a finished game. Code should be taught, not dumped.

Hard Rules for Any AI Assisting on This Project
No large/entire-game code dumps. Work in small, incremental steps — one script or one feature at a time.
One script = one responsibility. Don't merge concerns (e.g. don't put input-handling inside game-logic scripts — see InputTester.cs vs BlackjackGame.cs as the established pattern).
Explain before/alongside code. Concepts (classes, enums, lists, loops, etc.) should be introduced in plain language before or with the code that uses them.
The developer writes code independently when possible, then gets it diagnosed/corrected — not just handed finished scripts.
Don't rewrite working systems unnecessarily.
Placeholder-first. Ugly cubes/text before real art — visuals only come after logic works (see design doc Section 33).
Known recurring mistake pattern: the developer tends to introduce singular/plural naming typos (Card vs Cards) when copying/retyping code — this is the first thing to check on "type not found" errors.
Card Deck Spec (IMPORTANT — non-standard)

This is a Moroccan/Spanish-suited 40-card deck, NOT a standard 52-card deck:

Ranks: 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 (no 8s or 9s)
Traditional names used for face ranks: Sota (10), Caballo (11), Rey (12)
4 suits × 10 ranks = 40 cards total
Rank enum uses explicit integer values (One = 1, Two = 2, ... Rey = 12) specifically to handle the gap where 8/9 don't exist.
Blackjack Rule Variant (IMPORTANT — customized)
Standard Hit/Stand, dealer draws under 17, bust = auto-loss.
Ace scoring: DYNAMIC (1 or 11), not fixed. The developer initially found this confusing (score can visually "drop" when an Ace downgrades from 11 to 1 mid-hand) but after discussion chose to keep the dynamic version to match real Blackjack rules and the original design doc. Do not silently change this back to fixed-Ace-of-1 or vice versa without the developer's explicit request — it was a deliberate decision made after being shown the tradeoffs.
Lives system is symmetric, not player-only: both player AND dealer start with 5 lives. Whoever loses a round loses 1 life. Draws cost nobody a life. Match ends (GameState.GameOver) when either side hits 0.
Current Architecture / Scripts

All scripts live in Assets/Scripts/.

Script	Status	Responsibility
Card.cs	✅ Done, tested	Enum-based Suit/Rank, holds a single card's data + base Value. Pure data, no game logic.
Deck.cs	✅ Done, tested	Builds the 40-card deck (CreateDeck), Fisher-Yates Shuffle, DrawCard, ResetDeck. Knows nothing about UI or rules.
Hand.cs	✅ Done, tested	Holds a List<Card> via IReadOnlyList<Card> accessor. GetScore() implements dynamic Ace (1-or-11) logic via aceCount + while loop. IsBust(), IsBlackjack().
GameState.cs	✅ Done	Enum: Waiting, PlayerTurn, DealerTurn, RoundResult, GameOver.
BlackjackGame.cs	✅ Done, tested (console-only so far)	MonoBehaviour coordinator. Owns one Deck + two Hands (player/dealer) + playerLives/dealerLives (5 each). Methods: StartRound(), PlayerHit(), PlayerStand(), DealerTurn() (private), EvaluateRound() (private, applies life loss + checks GameOver). All public action methods guard on CurrentState before running.
InputTester.cs	✅ Done (temporary/throwaway)	Keyboard-only test harness — H = Hit, S = Stand, N = next round (StartRound()). Deliberately kept separate from BlackjackGame per the single-responsibility rule. Will be deleted once real UI buttons exist.
DeckTester.cs	✅ Done (temporary/throwaway)	Standalone regression test for Deck + Hand (Ace math) via Debug.Log. Not connected to BlackjackGame.
CardVisual.cs	✅ Done, tested (just now)	Takes a Card via SetCard(Card), displays Rank/Suit as text on the card GameObject. Must use 3D-world TextMeshPro, NOT TextMeshProUGUI/Canvas UI Text — this tripped up setup once already (wrong component type = silent failure, field won't accept the drag). No game-logic awareness.
CardVisualTester.cs	✅ Done (temporary/throwaway)	Spawns one CardVisual instance from a prefab reference at a fixed world position, feeds it a test Card. Proved the Card-data → visual pipeline works.
Scene State (Unity)
Placeholder cube as the table.
Placeholder cube as a card prefab, with CardVisual.cs attached and a child 3D Object → Text - TextMeshPro (not UI Text) wired to the Label field.
GameManager GameObject holding BlackjackGame + InputTester.
No real room, camera framing, lighting, or Moroccan art yet — intentionally postponed per design doc Section 33.
Progress Against the 5-Day Plan
Day 1 (pure C# Blackjack logic, console-tested): ✅ Complete. Full playable loop works via Debug.Log + keyboard test hooks: deal, hit, stand, dealer AI, win/lose/draw, symmetric 5-life system, game over, next round.
Day 2 (Unity gameplay — connect logic to visible/interactive scene): 🔶 In progress.
✅ Proven: a Card object can be turned into a labeled 3D object in the scene.
⬜ Not yet done: card position markers (empty Transforms for player/dealer slots).
⬜ Not yet done: BlackjackGame actually instantiating CardVisuals during StartRound()/PlayerHit()/DealerTurn() instead of only logging to Console.
⬜ Not yet done: Hit/Stand UI buttons (to replace/supplement InputTester).
⬜ Not yet done: on-screen score/lives display.
Day 3 (presentation: animations, room, lighting, real card art): ⬜ Not started.
Day 4 (atmosphere: sound, ambience, polish): ⬜ Not started.
Day 5 (bugfixing, title screen, restart, build): ⬜ Not started.