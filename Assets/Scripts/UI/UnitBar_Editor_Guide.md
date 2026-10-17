# Unit Bar UI — Unity Editor Build Guide

## Overview
Build the **Unit Bar** UI for DAMAS-DENIS using your existing custom grid system.
The bar displays all unlocked staff units, their stats, and allows selection for deployment.

---

## Part 1: Scripts (Done)

### Created Scripts

| Script | Purpose |
|---|---|
| `UnitDisplayData.cs` | UI-ready wrapper for `UnitDeploymentData` (display state, stats, selection) |
| `UnitSlot.cs` | Individual unit slot with portrait, stats, lock/cooldown overlays, click events |
| `UnitBar.cs` | Loads player staff set, populates slots, manages selection |
| `UnitBarManager.cs` | Orchestrates unit bar lifecycle, resource updates, stage transitions |

All scripts are in `Assets/Scripts/UI/`.

---

## Part 2: Unity Editor — Scene Setup

### Step 1: Create Unit Bar Canvas

1. **Create Canvas**  
   `GameObject > UI > Canvas`  
   Name it `UnitBarCanvas`

2. **Set Canvas Scaler** (if needed)  
   Set `UI Scale Mode` to `Scale With Screen Size`  
   Set `Reference Resolution` to `1920 x 1080` (or your target)

3. **Create Unit Bar Container**  
   Under Canvas, create an empty GameObject  
   Name it `UnitBar`  
   Add `UnitBar` script component

4. **Create Unit Slot Prefab**  
   Create a new prefab: `UI > Prefab`  
   Name it `UnitSlotPrefab`
   
   - Add the prefab to `UnitBar.unitSlotPrefab` in Inspector
   - Set `unitSlotContainer` to the parent of the prefab instances

### Step 2: Build the Unit Slot UI

1. **Create Slot GameObject**  
   Under UnitBar, create an empty GameObject  
   Name: `UnitSlot_01`

2. **Add Image**  
   `UI > Image`  
   Set the Source Image to a unit portrait sprite (or leave empty for now)

3. **Add TextMeshPro components**  
   Add `TextMeshPro - Text` for:
   - Name (unit name)
   - Cost (placement cost)
   - Stats (damage, range, speed, health, type)
   
   Add `Button` components for:
   - Select button (click to deploy)
   - Upgrade button

4. **Add Container Transform**  
   The `unitSlotContainer` in `UnitBar` script should be the parent of all slot instances.

### Step 3: Setup Unit Bar Slot Layout

Example slot hierarchy:
```
UnitBar (UnitBar script)
├── UnitSlot_01 (UnitSlot prefab)
│   ├── Image (portrait)
│   ├── Text (name)
│   ├── Text (cost)
│   ├── Text (stats)
│   ├── Button (select)
│   └── Button (upgrade)
├── UnitSlot_02
├── UnitSlot_03
└── ...
```

---

## Part 3: Unity Editor — Script Setup

### Step 4: Assign References

1. **UnitBar script** (on `UnitBar` GameObject)
   - `unitSlotContainer` → drag the container Transform
   - `unitSlotPrefab` → drag `UnitSlotPrefab`
   - `playerState` → drag `PlayerState` GameObject
   - `researchCreditsText` → drag the TMP text element
   - `goldText` → drag the TMP text element
   - `emptyStateText` → drag the empty state TextMeshPro

2. **UnitBarManager script** (on a manager GameObject, e.g., `UnitBarManager`)
   - `unitBar` → drag `UnitBar` GameObject
   - `stageState` → drag `StageState` GameObject
   - `playerState` → drag `PlayerState` GameObject
   - `currentStage` → drag `Stage 1.asset` (StageDefinition)
   - `researchCreditsText` → drag the TMP text
   - `goldText` → drag the TMP text

3. **UnitSlot script** (on each slot)
   - `portraitImage` → drag Image component
   - `nameText` → drag TextMeshPro
   - `costText` → drag TextMeshPro
   - `damageText`, `rangeText`, `speedText`, `healthText`, `typeText` → drag TextMeshPro
   - `selectButton` → drag Button
   - `upgradeButton` → drag Button
   - `lockOverlay`, `cooldownOverlay` → drag Image components
   - `onSelect` → assign event (call `UnitBar.OnSlotSelected`)

### Step 4: Create Unit Data Assets

1. **Create UnitDeploymentData assets**  
   Right-click in Project window → `DAMAS DENIS > Deployment Unit`  
   Name: `Staff 1`, `Staff 2`, `Staff 3`

2. **Configure each unit**  
   Fill in:
   - `DisplayName` (e.g., "Ranger", "Medic", "Shield")
   - `UnlockId` (e.g., "staff.ranger", "staff.medic")
   - `UnitPrefab` → reference the staff prefab
   - `DeploymentCost` (e.g., 10)
   - `AttackType` (Melee / Ranged)
   - `Damage`, `Range`, `AttackSpeed`, `MaxHealth`
   - `DeployOnGround`, `DeployOnElevated`

3. **Create StageDefinition** (already exists as `Stage 1.asset`)
   - Assign `UnlockedStaff` for optional stage rewards

### Step 5: Load Units in Editor

1. **Add test units**  
   Create 3-5 unit data assets in `Assets/Resources/` (for the prototype)

2. **Run the scene**  
   Press Play

3. **Check console**  
   Should see: `"Unit Bar Manager initialized."`

4. **Verify slots**  
   UnitBar should show all unlocked units, sorted by cost

---

## Part 4: Integration with Core Loop

### Step 6: Connect to Deployment

1. **In UnitSlot.OnSelectClick()**  
   Call `DeploymentController.BeginPlacement(unitData)`

2. **In UnitBarManager**  
   After stage load, filter units by:
   - `unit.isUnlocked`
   - `unit.CanDeployOn(cell.terrain)` for the stage map

### Step 7: Update Resources

1. **Subscribe to PlayerState.OnResearchCreditsChanged**  
   Updates research credits display

2. **Subscribe to StageState.OnStageResourceChanged**  
   Updates gold/placement cost display

---

## Part 5: Editor Checklist

| Item | Status |
|------|--------|
| UnitDisplayData.cs created | ✅ |
| UnitSlot.cs created | ✅ |
| UnitBar.cs created | ✅ |
| UnitBarManager.cs created | ✅ |
| UnitSlot prefab created | ⏳ |
| Unit data assets created | ⏳ |
| Reference assignments done | ⏳ |
| Unit bar loads on play | ⏳ |
| Selection works | ⏳ |
| Deployment integration done | ⏳ |

---

## Notes

- **Resources folder**: `Resources.LoadAll<UnitDeploymentData>("")` loads from `Assets/Resources/`
- **Test units**: Filter out units with "Test" in name
- **Stage filtering**: `GetDeployableUnits()` filters by unlocked status and optional stage rewards
- **HUD layout**: Adjust spacing, font sizes, and layout groups to fit your game's art style
