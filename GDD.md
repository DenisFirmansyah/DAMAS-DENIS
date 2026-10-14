# GDD: [Working Title TBD]

**Team:** SunSaKi | **Engine:** Unity | **Genre:** Tower Defense | **Platform:** Multiplatform | **Audience:** Teens and up
**References:** Kingdom Rush (economy, upgrades), Arknights (map placement, stage-edge story)

## Pitch

You are a handler at a corporation's secret research facility. Each stage, enemies rush the base. You place and upgrade staff to hold the line until a timer runs out and the facility relocates. A boss gives you your orders and everything you know about the enemy. Optional objectives unlock stronger staff. In the last stage, the research completes and reaches society.

Lose a stage and the research is taken and the staff are killed. Win and the facility moves on.

## Design Pillars

1. **Fun first.** It must play well as a tower defense on its own.
2. **Defend is the only order.** Not defending means losing. The player never gets a button that disobeys.
3. **Story lives in the world.** Text appears at stage open and close. UI, map design, and characters carry the rest.

## Core Loop

1. Boss briefing (dialogue)
2. Place staff on the map
3. Survive timed waves, with optional objectives running alongside
4. Facility relocates, stage ends with a quiet text cutscene
5. Spend stage resources on upgrades, unlock units from completed objectives
6. Next stage

## Mechanics

- **Placement:** Arknights-style map placement.
- **Economy:** Each stage pays resources, spent on Kingdom Rush-style unit upgrades.
- **Removal:** Pulling a unit out takes processing time and has requirements. Example: a squad leaves a position only with a medic present.
- **Unlocks:** New staff units unlock by completing optional objectives.
- **Win:** Timer ends and the facility relocates.
- **Lose:** The base falls.
- **Reset:** Each stage resets. No state carries over (prototype only).

## Units and Enemies

- **Units:** Staff only. Roles in OPEN_NOTES.
- **Enemies:** They rush the base. Subtle implications appear at stage start through boss dialogue, enemy movement, and enemy design.

## Story and Presentation

- The player's knowledge about the enemy comes only from the corporation.
- A late twist recasts the enemies. The player cannot act on it. The target feeling is regret.
- Presentation follows Arknights: text at stage opening and ending, everything else told through UI, map, and characters.
- A review at the end shows the player their choices against what actually happened.

## Jam Goal

The game never states its message. Players follow orders as a mechanic, then see their own choices set against the outcome in the review. Which pathway elements the game highlights is open (see OPEN_NOTES).

## Prototype Scope (2 weeks)

**Done:** Basic unit placement, enemy spawner.

**Build:**
- One stage with a timer and waves
- Two or three staff units with upgrades from stage resources
- One optional objective that unlocks a unit
- Boss dialogue at stage start
- Quiet text cutscene at stage end
- Retreat with processing time and one requirement
- Log of player choices for the review

## Team and Timeline

2 main developers, 1 to 2 part-time. GDD in 2 days, prototype in 2 weeks, later milestones TBD.
