# Clash of the Elements, a 2D Online Trade Card Game

This app served as my ***degree project*** and was inspired by an old TCG called **Duel Masters** that people used to play with physical cards. It features cards of 2 types **creatures** and **spells** having special effects, used to destroy your opponent's **safeguards** and attack him one more time in order to win.

# Gameplay and Rules

This rulebook describes the rules exactly as the game code enforces them. The rules engine runs on the client (`DM - Client/DM - Client/GamePlay/*.cs`, all partial classes of `GUIGameRoom`). The server (`DM - Server/DM - Server/Models/InGameData/Game.cs`, `Controllers/ControllerOnGoingGame.cs`) holds the hidden information (deck order, safeguards, hands), performs draws and safeguard breaks, relays every action to the opponent and records match results.

## Glossary

The engine uses its own vocabulary. The table maps each term to its Duel Masters equivalent; the card texts stored in the database use the Duel Masters keywords in brackets.

| Clash of the Elements (code) | Duel Masters | Meaning |
|---|---|---|
| Element (`Card.Element`) | Civilization | One of five card affiliations |
| Safeguard / Safeguard zone | Shield / Shield zone | Face-down cards protecting a player |
| Battle ground / battle zone | Battle zone | Where creatures (and resolving spells) sit |
| Mana zone | Mana zone | Resource pool built from cards in hand |
| Engaged (`isEngaged = true`) | Tapped | Used this turn (mana spent, creature attacked or blocked) |
| Disengaged | Untapped | Ready to use |
| `hasCompletelyBeenSummoned = false` | Summoning sickness | Creature cannot attack yet |
| `Defender` | [Blocker] | May intercept attacks |
| `Slippery` | Can't be blocked | Ignores blockers |
| `Poisonous` | [Slayer] | Destroys whatever beats it in battle |
| `SafeguardBreaker` | [Double breaker] etc. | Breaks extra safeguards |
| `BloodThirst` | [Power attacker +N] | Bonus power while attacking |
| `SafeguardActive` trigger | [Shield trigger] | Usable for free when revealed from a broken safeguard |

## Card Anatomy

Every card (`Models/Collection/Card.cs`) carries the following fields:

| Field | Type | Role in the rules |
|---|---|---|
| `ID` | int | Unique card identifier (1-based) |
| `Name`, `Set`, `Race`, `Text` | string | Descriptive; `Text` is the human-readable rules text |
| `Type` | `"Creature"` \| `"Spell"` | Determines whether the card stays in the battle zone |
| `Element` | `"Fire"` \| `"Water"` \| `"Nature"` \| `"Darkness"` \| `"Light"` | Mana colour requirement (see [The Elements](#the-elements)) |
| `Cost` | int | Number of mana cards that must be engaged to play it |
| `Power` | int | Combat strength of a creature; `-1` for spells |
| `SpecialEffects` | list | Zero or more keyword abilities / triggered effects |

During a match, cards are wrapped in `CardWithGameProperties`, which adds the two pieces of per-game state the engine tracks:

- `isEngaged`: whether the card is tapped.
- `hasCompletelyBeenSummoned`: whether a creature has survived until its controller's next turn and may attack.

## Deck Construction

Enforced by `Models/Collection/CollectionDeck.cs` and `GUIPages/GUICollection.xaml.cs`:

- At most **4 copies** of any single card (by `ID`).
- At most **40 cards** per deck.
- There is no minimum deck size. Any saved deck can be selected in the pre-game room.

## Match Setup

1. **First player.** When the match is created, the server draws `r.Next(1, 10000)` and makes Player 1 go first if the value is even. Otherwise Player 2 goes first, giving an effectively 50/50 coin flip.
2. **Shuffle.** Each player's deck list is expanded into individual cards and shuffled with a Fisher–Yates shuffle driven by `RNGCryptoServiceProvider` (rejection-sampled to avoid modulo bias).
3. **Safeguards.** The top **5** cards of the shuffled deck are placed face down as the player's safeguards, numbered 1–5.
4. **Opening hand.** The next **5** cards are drawn into the player's hand.
5. Both clients signal `READYTOSTART`; once both are ready, the first player begins their first turn.

## Turn Structure & Phases

A turn runs through the following phases in a fixed order. A phase can only move forward; there is no way to return to an earlier one.

```
Start of turn ─► Mana phase ─► Summon phase ─► Attack phase ─► End of turn
  (automatic)    (≤ 1 card)    (any number)    (each ready      (opponent's
                                               creature once)    turn begins)
```

### 1. Start of Turn (automatic — `GamePlay/StartTurn.cs`)

Executed in this order when the server sends `YOURTURN`:

1. **Spell cleanup.** Every Spell still lying in either player's battle zone (e.g. a shield-trigger spell the player cast during the opponent's turn) is moved to its owner's graveyard.
2. **Untap.** All of the active player's mana cards and battle-zone cards become disengaged.
3. **Summoning sickness wears off.** Every creature the active player controls gets `hasCompletelyBeenSummoned = true` and may attack this turn.
4. **Draw.** The server draws the top card of the active player's deck into their hand (`STARTTURN` → `ROLLON`).
5. The **Mana phase** begins.

> **First-turn rule:** The player who goes first skips the whole Start-of-Turn step on their first turn. Their client goes directly to the Mana phase after the opening animations, so **the first player does not draw on turn 1**. The second player's first turn is a normal turn and includes the draw.

### 2. Mana Phase (`GamePlay/ManaPhase.cs`)

- The active player may choose **exactly one** card from their hand and put it into their mana zone ("Play as Mana").
- Any card (creature or spell, any element) can be played as mana.
- Playing mana is optional. **Next Phase** skips it.
- After a card is played as mana, the game advances to the Summon phase automatically. A second mana card in the same turn is impossible.

### 3. Summon Phase (`GamePlay/SummonPhase.cs`)

- The active player may play **any number** of cards from their hand, one at a time, as long as each can be paid for (see [Resource System](#resource-system)).
- **Creatures** enter the battle zone with summoning sickness (`hasCompletelyBeenSummoned = false`) unless they have `InstantSummon`.
- **Spells** are also placed in the battle zone while their effects resolve. They stay there until the next end-of-turn/start-of-turn cleanup, then go to the graveyard.
- Any effect with the `Summon` trigger fires as soon as the card is played (this is how all spell effects and "when you put this creature into the battle zone" abilities work).
- A card drawn during the Summon phase (e.g. via a `Draw` effect) can be played in the same phase.
- **Next Phase** moves on to the Attack phase.

### 4. Attack Phase (`GamePlay/AttackPhase.cs`)

**Eligible attackers:** creatures in the active player's battle zone with `hasCompletelyBeenSummoned = true` and without `CannotAttack`. Each eligible creature may attack **at most once** per turn, and attacking engages (taps) it.

The available actions depend on the opponent's safeguard count:

| Opponent safeguards | Available attack buttons |
|---|---|
| ≥ 1 | **Attack Safeguards**, **Attack Creatures**, **End Turn** |
| 0 | **Attack Opponent!**, **Attack Creatures**, **End Turn** |

The button set switches to **Attack Opponent!** as soon as the opponent's last safeguard is broken into their hand, even in the middle of the phase. If that last safeguard is activated as a shield trigger instead, the button appears at the start of the next Attack phase.

#### 4a. Attacking safeguards

1. The attacker chooses a number of the opponent's safeguards equal to the creature's **break count**:

   ```
   breakCount = min(1 + SafeguardBreaker.Arguments[0], opponentSafeguards)
   ```

   A creature without `SafeguardBreaker` breaks 1 safeguard; a `SafeguardBreaker(1)` creature ("Double breaker") breaks 2, and so on.
2. **Block window.** If the attacker is not `Slippery` and the defender has at least one untapped `Defender` creature, the defender may block with one of them (see [Blocking](#blocking)).
3. If the attack is **not blocked**, the chosen safeguards break:
   - If more than one safeguard breaks, the **defending** player chooses the order in which they are revealed.
   - Each revealed card goes to its owner's **hand**, unless it has a `SafeguardActive` trigger (shield trigger) and the owner chooses to **activate** it. An activated card is put into the owner's battle zone **without paying its cost**, and its `SafeguardActive` effects resolve immediately (still during the attacker's turn).

#### 4b. Attacking creatures

- Only **engaged (tapped)** opposing creatures can be targeted.
- The defender may block with an untapped `Defender` (unless the attacker is `Slippery`), which changes the target to the blocker.
- The attacker and the target (or blocker) then **battle** (see [Combat Resolution](#combat-resolution)).

#### 4c. Attacking the opponent directly

- Only available when the opponent has **0 safeguards**.
- The defender may block with an untapped `Defender` (unless the attacker is `Slippery`), which resolves as a normal battle.
- If the attack is **not blocked**, the attacking player **wins the game**.

#### Blocking

- A blocker must be a creature in the defending player's battle zone with the `Defender` effect that is **not engaged**.
- Blocking **engages** the blocker, so each blocker can block at most once per opponent turn.
- Only one blocker can be chosen per attack.
- An attacker with `Slippery` cannot be blocked at all.
- When one of the defender's creatures is attacked, the defender can block with a *different* `Defender` creature, which then fights the battle instead.

### 5. End of Turn (`GamePlay/EndTurn.cs`)

The turn can only end from the Attack phase (**End Turn**). Executed in this order:

1. Every Spell in either battle zone moves to its owner's graveyard.
2. The opponent's mana and battle-zone cards are untapped. This mirrors the untap at the start of the opponent's turn so both clients show the same board.
3. The turn passes (`ENDTURN` → opponent receives `YOURTURN`).

There is **no maximum hand size** and no discard step.

## Resource System

### Building mana

Mana comes only from cards placed in the mana zone: at most one per turn during the Mana phase, plus card effects such as `SendTo OwnDeck → OwnMana` (put the top card of your deck into your mana zone) or the `Recyclable` replacement effect. A mana card stays in the mana zone permanently unless an effect moves it, and every mana card produces mana of its own `Element`.

### Paying a cost

Validated by `checkCanSummon` (`GamePlay/SummonPhase.cs`) and the mana selection dialog (`GUIWindows/GUISelect.xaml.cs`).

A card with cost `C` and element `E` can be played only if **both** conditions hold over the player's untapped mana cards `U`:

```
|U| ≥ C                                        (enough untapped mana)
∃ m ∈ U : m.Element = E                        (at least one mana card matches the card's element)
```

The player then chooses **exactly `C`** untapped mana cards to engage, and **at least one** of the chosen cards must match `E`. The rest can be any element. Engaged mana stays engaged until the start of the player's next turn.

Example: a Fire card with cost 5 needs 5 untapped mana cards, at least one of them Fire (e.g. 1 Fire + 4 Water).

### Deploying to the board

- Paying the cost moves the card from hand to the battle zone (creatures and spells alike).
- Shield-trigger activation (`SafeguardActive`) is the only way to put a card into play **without** paying its cost.
- Effects can also put cards into play from other zones (e.g. `SendTo OwnGrave → OwnGround`, which returns a creature from the graveyard to the battle zone).

## The Elements

There are five elements. Some effect arguments refer to them by a numeric code (`codeToCivilization` in `GamePlay/SpecialEffects.cs`):

| Code | Element |
|---|---|
| 1 | Fire |
| 2 | Water |
| 3 | Nature |
| 4 | Darkness |
| 5 | Light |

### How elements interact

**Elements have no direct combat interaction.** The engine has no type chart, weakness, resistance or damage multiplier between elements. A Fire creature battling a Water creature resolves exactly like any other battle: by comparing power alone.

Elements affect the game in only two places:

1. **Mana colour requirement.** Playing a card requires at least one engaged mana card of the card's element (see [Paying a cost](#paying-a-cost)). This is what makes deck element composition matter.
2. **Element-scaled power (`BloodThirstRevenge`).** With condition `OwnGraveyardCivilization`, a creature gets bonus power while attacking for every card of a given element in its controller's graveyard:

   ```
   bonus = Arguments[0] × |{ c ∈ OwnGraveyard : c.Element = codeToCivilization(Arguments[1]) }|
   ```

   For example, `BloodThirstRevenge(1000, 1)` gives **+1000 power per Fire card** in your graveyard while attacking.

## Combat Resolution

Implemented in `Battle`, `getCreaturePowerWhileAttacking` and `killCreature` in `GamePlay/AttackPhase.cs` and `GamePlay/SpecialEffects.cs`.

### Power calculation

Attack bonuses apply **only to the attacking creature**. The defending creature (the target or the blocker) always uses its printed power.

```
P_attacker = Power
           + Σ BloodThirst.Arguments[0]
           + Σ BloodThirstRevenge bonus (see The Elements)

P_defender = Power
```

### Outcome

| Comparison | Result |
|---|---|
| `P_attacker > P_defender` | Defender is destroyed |
| `P_attacker < P_defender` | Attacker is destroyed |
| `P_attacker = P_defender` | Both are destroyed |

After the winner is determined, two keywords can also destroy the winner:

- **`Poisonous`** (Slayer): if the *losing* creature has `Poisonous`, the winning creature is destroyed too.
- **`OnePunch`**: if the *winning* creature has `OnePunch`, it is destroyed after winning.

On a tie both creatures are already destroyed, so neither keyword changes anything.

The target of a creature attack is always engaged, and a blocker is engaged when it blocks, so creature combat only ever involves tapped defenders.

### Destruction and replacement effects

Whenever a creature is destroyed (in battle or by a "destroy" effect), the engine checks these in priority order:

1. `Unkillable`: the creature returns to its owner's **hand** instead.
2. `Recyclable`: the creature goes to its owner's **mana zone** instead (adding to that player's mana).
3. Otherwise it goes to its owner's **graveyard**.

## Win / Loss Conditions

A match ends in exactly one of these ways:

| Condition | Result | Mechanism |
|---|---|---|
| A creature attacks the opponent directly while they have **0 safeguards** and the attack is **not blocked** | Attacker wins | The defending client concedes automatically (`ISURRENDER`) |
| A player clicks **Surrender** (confirmed) | That player loses | Client sends `ISURRENDER` |

When the server receives `ISURRENDER`, it records the result (`GamesWon + 1` for the winner, `GamesLost + 1` for the loser in the `UserData` table), removes the game and notifies the winner with `OPPSURRENDERED`.

To win, a player must break every opposing safeguard (one attack can break several with `SafeguardBreaker`) and then land one unblocked direct attack.

### Conditions that do **not** end the game

- **Running out of cards.** There is no deck-out loss. When the deck is empty, `Game.drawCard` returns the sentinel `-1` instead of a card ID, and the client has no rule that handles it.
- **Disconnection.** A disconnected player's game is removed on the server (`removeGameByClient`) **without** recording a win or a loss.

## Special Effects Reference

Each `SpecialEffect` has `Effect`, `Trigger`, `Condition`, `Arguments`, `TargetFrom` and `TargetTo`. A card can have several.

### Triggers

| Trigger | Fires when |
|---|---|
| *(none)* | Static ability, always active |
| `Summon` | The card is played from hand (creature enters play, or spell is cast) |
| `SafeguardActive` | The card is revealed from a broken safeguard and its owner activates it (shield trigger) |

### Static keywords

| Effect | Arguments | Rule |
|---|---|---|
| `Defender` | — | Can block (see [Blocking](#blocking)) |
| `Slippery` | — | Cannot be blocked |
| `CannotAttack` | — | Never an eligible attacker |
| `SafeguardBreaker` | `[n]` | Breaks `1 + n` safeguards per attack |
| `BloodThirst` | `[x]` | `+x` power while attacking |
| `BloodThirstRevenge` | `[x, elementCode]`, condition `OwnGraveyardCivilization` | `+x` power while attacking for each card of that element in your graveyard |
| `Poisonous` | — | Destroys the creature that beats it in battle |
| `OnePunch` | — | Destroyed after winning a battle |
| `Unkillable` | — | Returns to hand instead of being destroyed |
| `Recyclable` | — | Goes to mana zone instead of being destroyed |

### Triggered effects

| Effect | Arguments | Rule |
|---|---|---|
| `InstantSummon` | — | The creature has no summoning sickness and can attack the turn it is played |
| `Draw` | `[n]` | Draw `n` cards from the top of your deck |
| `SendTo` | `[count]` or `[count, powerCap]` | Move up to `count` cards from `TargetFrom` to `TargetTo` (below) |

#### `SendTo` details

- `count = 23` (`ALL`) means **every** valid card in the source zone, chosen automatically.
- Otherwise the player picks `min(count, available)` cards; this choice cannot be cancelled.
- Condition `LessOrEqual` restricts targets to **creatures** with `Power ≤ Arguments[1]`.
- For `Any*` sources the count is shared between both players' zones (e.g. "choose up to two creatures in the battle zone").
- Destroying effects (`→ OwnGrave` / `→ OppGrave` from a battle zone) go through the destruction replacement rules (`Unkillable`, `Recyclable`).

Implemented routes:

| `TargetFrom` → `TargetTo` | Meaning |
|---|---|
| `OwnDeck` → `OwnMana` | Put the top `count` cards of your deck into your mana zone |
| `OwnMana` → `OwnGrave` | Put cards from your mana zone into your graveyard |
| `OwnGround` → `OwnGrave` | Destroy your own creature(s) |
| `OppGround` → `OppGrave` | Destroy opponent's creature(s) |
| `OwnMana` → `OwnHand` | Return cards from your mana zone to your hand |
| `OwnGrave` → `OwnHand` | Return cards from your graveyard to your hand |
| `OwnGround` → `OwnHand` | Return your creature(s) to your hand |
| `OppMana` → `OppHand` | Return opponent's mana cards to their hand |
| `OppGround` → `OppHand` | Return opponent's creature(s) to their hand |
| `AnyGround` → `OwnersHand` | Return creatures from either battle zone to their owners' hands |
| `OwnGrave` → `OwnGround` | Put cards from your graveyard into your battle zone |

## Turn Flow Summary

```
                    ┌──────────────────────────────────────────────┐
                    │ START OF TURN (skipped on the 1st player's    │
                    │ first turn)                                   │
                    │  1. Spells in play → graveyard                │
                    │  2. Untap own mana + creatures                │
                    │  3. Remove summoning sickness                 │
                    │  4. Draw 1                                    │
                    └───────────────────┬──────────────────────────┘
                                        ▼
                    ┌──────────────────────────────────────────────┐
                    │ MANA PHASE: put ≤ 1 card from hand into mana │
                    └───────────────────┬──────────────────────────┘
                                        ▼
                    ┌──────────────────────────────────────────────┐
                    │ SUMMON PHASE: play any number of cards        │
                    │  pay C mana, ≥ 1 of the card's element        │
                    └───────────────────┬──────────────────────────┘
                                        ▼
                    ┌──────────────────────────────────────────────┐
                    │ ATTACK PHASE: each ready creature may         │
                    │  • break safeguards   • attack tapped creature│
                    │  • attack player (only at 0 safeguards → win) │
                    └───────────────────┬──────────────────────────┘
                                        ▼
                    ┌──────────────────────────────────────────────┐
                    │ END OF TURN: spells → graveyard, pass turn    │
                    └──────────────────────────────────────────────┘
```

### Interface
![](https://github.com/Gabriel-Em/Clash-of-the-Elements___Online-2D-TCG/blob/master/DM%20-%20Client/DM%20-%20Client/Resources/GameInterface.png)

# Building and running

#### Prerequisites
- Visual Studio (2015 or newer)

#### Compiling

You need to restore some NuGet packages before compiling. Follow these steps:

Open the **Package Manager Console** in Visual Studio:
- Tools -> Nuget Package Manager -> Package Manager Console

Execute:
```
PM> Install-Package NuGet.CommandLine
PM> nuget restore
```
Do that for both projects. Reopen the projects if neccessary.

Now you can build and run the projects.

# Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

# Contact
Feel free to contact me via email - gabriel.pop@yandex.com

# License
- **[MIT license](http://opensource.org/licenses/mit-license.php)**
