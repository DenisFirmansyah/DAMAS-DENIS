TODO (Agent Edition)
How to use this file

Read this whole file before starting any task. Work on one task at a time, in the order given unless the user says otherwise.

Rules for the agent
Ask before guessing. If a task has an open question (marked ASK:), or you hit a decision that is not covered here, stop and ask the user. Use AskUserQuestion with 2-4 concrete options and put your recommended option first. Do not invent design values, narrative text, or art direction.
Do not ask about things the code already answers. Read the relevant scripts first, then ask only what is genuinely the user's call.
Data-driven only. Gameplay values live in ScriptableObjects (UnitDeploymentData, EnemyData, StageDefinition, StaffUpgradeDefinition, AttackRangePattern) or other external data. Never hardcode balance numbers in scripts.
Prefer dependency injection over new singletons. Use [SerializeField] references. If a singleton is truly the simplest option, explain why and ask first.
All public APIs get XML doc comments.
Explain decisions. For each non-trivial choice, say what you chose, why, and what the alternative was.
Unity Inspector steps. Whenever a task needs scene, prefab, or asset setup, give a numbered step-by-step guide (which object, which component, which field, which value). Do not assume the user knows where things are.
Custom code needs a teaching pass. If a task needs a new script that is not trivial, first propose the approach and ask the user. Then explain how it works after writing it.
Use the existing event log. Player-facing events go through StageEventLog (stage) or RoamingEventLog (roaming) so the review can read them. Do not create a parallel logging system.
Stay in scope. Out of scope per the GDD: persistent campaign state, branching narrative, inventory, complex upgrade trees, second maps. If a task seems to need one of these, ask.
When done with a task, summarize in 2-3 sentences what changed, list the files touched, and list any Inspector steps the user still has to do.
Project context
Unity, C#, namespace DamasDenis.*. Hybrid 3D map with 2D billboard sprites (Billboard).
Tower defense loop: StageDirector (briefing, battle, ending), WaveManager, DeploymentController, StaffCombat, StaffBlocker, StaffRetreatController.
Scenes: MainMenu, Lobby, Gameplay (plus additive stage scenes), Roaming, Review. Scene flow is in GameManager.
Game design intent is in GDD.md. The game never states its message; the review compares player choices with outcomes.
1. Game balancing (unit stats, enemy stats, stages)

Goal: Stages are winnable but tense, and upgrades feel meaningful.

Where the values live: UnitDeploymentData (cost, damage, attack speed, health, block count), EnemyData (health, speed, damage, reward, attack interval), StageDefinition (survival seconds, base health, starting and completion resources), StaffUpgradeDefinition, WaveManager.waves in each stage scene.

Steps:

Read all existing unit, enemy, stage, and wave assets and produce a table of current values.
Identify obvious problems (a unit never worth its cost, enemies that cannot be stopped, resource income too high or low).
Propose changes as a before/after table. Do not apply until the user approves.
Apply changes only in data assets, never in code.

ASK:

Target difficulty and intended stage length per stage?
How many stages are being balanced right now?
Does the user want to playtest between rounds, or should the agent do a paper balance pass first?
Should the "third star" objective (see task 3) affect the balance targets?
2. Weapon visuals

Goal: Staff attacks are visually readable.

Context: Ranged units use projectilePrefab + Projectile (homing). Melee units hit directly in StaffCombat.Attack with no visual. Note that Projectile moves on the XY plane while the map is on XZ, so check that projectiles look right in the hybrid 3D setup before adding visuals.

Steps:

Check how projectiles currently render and travel on the 3D map. Report any problems.
Propose a minimal visual set (projectile sprite, melee swing/hit effect, muzzle or impact effect).
Build the prefabs and wire them through UnitDeploymentData.

ASK:

Which units need visuals first?
Does the user already have art assets, or should the agent use placeholders (simple shapes/particles)?
Melee: slash effect, hit flash, or animation trigger?
Should visuals be tied to audio cues (see task 7)?
3. Stage polish

Goal: Each stage looks finished and carries its story.

Sub-tasks:

Map and background: Polish the stage scene visuals and background.
Dialogue: Fill StageDefinition.briefing and relocationDialogue using DialogueData assets.
Third-star narrative objective: An optional objective that awards a third star.

Context: There is currently no star system in the code. StageDefinition has an optional-stage reward (unlockedStaff) but no stars or objective tracking. Roaming already has objective-style events in RoamingEvents.

ASK before building:

Who writes the dialogue? Does the user provide the text, or should the agent draft it for review? (Never finalize narrative text without approval.)
What exactly is the third-star narrative objective for each stage, and how is it detected (an event in StageEventLog, a threshold, a choice)?
Are stars stored per stage and shown in the lobby? Where should they persist (PlayerPrefs like StageRecordStore)?
Background art: existing assets or placeholders?
How many stages are in scope for this pass?
4. Roaming mode: finish the mode and its objective

Goal: Playable roaming loop: grab parts, carry them from one point to another, explore to find secrets.

Context: Roaming.unity has a Roamer (CharacterController), a camera follower, a door gated by progress ID, an NPC, and collectibles. RoamingEvents defines EvidenceCollected, AllEvidenceCollected, RoomUnlocked, PartsSent, PartsDelayed. EvidenceRecordStore saves results.

Steps:

Read the roaming scripts and scene, then write a short status report: what works, what is missing.
Propose the objective flow (pickup, carry, delivery, success/failure) and wait for approval.
Implement in small, testable pieces.

ASK:

Carry rules: one part at a time or several? Can the player drop parts? Is there a time limit (what does PartsDelayed mean in gameplay terms)?
What are the secrets, and what do they give the player (story, unlocks, evidence)?
What is the win/fail condition, and how does the result link back to the tower defense or the review?
Should roaming outcomes feed StageEventLog, EvidenceRecordStore, or both?
5. Roaming mode UI/HUD

Goal: The player can always see objective state in roaming.

ASK:

What must the HUD show (objective text, parts carried, delivery target, evidence count, interact prompt, timer)?
Match the Gameplay HUD style or a different look?
Should it be a UIScreen subclass like the other screens? (Recommended for consistency.)

Give Inspector steps for the Canvas, anchors, and references.

6. Roaming map model polish

Goal: The roaming map looks like a real facility instead of cubes.

Context: The current map is built from Unity cubes.

ASK:

Are final 3D models available, or should the agent do a greybox cleanup (materials, lighting, layout)?
Art style and reference?
Performance target (platforms)?
The agent cannot create 3D models. It can set up materials, lighting, colliders, and prefab organization.
7. SFX control and audio

Goal: Working volume controls plus real sound effects.

Context: GameSettings (saved volumes), AudioManager (PlaySFX, music per scene), and OptionsScreen (sliders) already exist. The sliders are wired in code, but no gameplay system calls PlaySFX yet, and the clips are not assigned.

Steps:

Verify the existing sliders actually change volume end to end and report any bugs.
Propose a list of SFX events (deploy, retreat, attack, enemy death, base hit, UI click, dialogue, stage win/lose, roaming pickup/delivery).
Hook them up. Prefer a data-driven approach (a ScriptableObject of named clips) over hardcoded clip fields.

ASK:

Which audio assets exist already? Should the agent use placeholders?
Which events matter most for this release?
Should the Options screen be reachable from the pause menu too?