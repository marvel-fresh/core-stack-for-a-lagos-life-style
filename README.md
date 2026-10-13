# Lagos Life-Style: Core Game Stack & Architecture

Cross-platform (Mobile + PC) open-world game architecture set in Lagos, Nigeria. Built with Unity (C#) and the Universal Render Pipeline (URP).

---

## 🌟 Core Stack Overview

- **Game Engine:** Unity (C#) using **URP (Universal Render Pipeline)**.
- **3D Modeling & Props:** Blender (low-poly Danfo minibuses, Keke NAPEP, roadside kiosks, traffic barriers).
- **Character Rigging & Animations:** Mixamo with root motion / procedural blend trees.
- **Input Architecture:** Unified cross-platform input abstraction mapping seamlessly to **PC (Keyboard/Mouse, Gamepad)** and **Mobile (Virtual Touch Joystick & UI Action Buttons)**.
- **Physics:** Unity `WheelCollider` suspension and torque physics with lowered Center of Mass for stable Danfo driving.
- **Audio:** Afrobeats radio broadcast system, Lagos traffic horns, and ambient street sounds.

---

## 📁 Repository Structure

```text
├── Assets/
│   └── Scripts/
│       ├── Input/
│       │   ├── InputManager.cs         # Centralized cross-platform input hub
│       │   ├── VirtualJoystick.cs      # Floating & fixed UI drag joystick
│       │   ├── TouchActionButton.cs    # Jump, Sprint, Enter Danfo, Handbrake
│       │   └── MobileControlsToggle.cs # Auto-detects mobile vs PC & switches HUDs
│       ├── Player/
│       │   ├── PlayerController.cs     # Third-person character controller
│       │   └── PlayerInteraction.cs    # Proximity detector (Danfo, Vendors, NPCs)
│       └── Vehicles/
│           ├── CarController.cs        # 4-wheel physics, steering, torque, Lagos horn
│           └── VehicleEntryExit.cs     # Seamless vehicle enter/exit & camera swap
├── index.html                          # Interactive web dashboard & input simulator
└── README.md
```

---

## 🎮 How the Cross-Platform Input System Works

### 1. Unified Input Manager Pattern (`InputManager.cs`)
Neither the player character nor the vehicle reads raw keyboard or touch inputs directly. Instead, both query:
```csharp
Vector2 move = InputManager.Instance.MoveInput;
bool isSprinting = InputManager.Instance.IsSprinting;
bool jump = InputManager.Instance.JumpTriggered;
bool interact = InputManager.Instance.InteractTriggered;
```
- **On PC:** Reads `Horizontal`, `Vertical`, `LeftShift`, `Space`, `E`/`F`.
- **On Mobile:** The `VirtualJoystick` and `TouchActionButton` components feed into `InputManager.Instance.SetVirtualMove(...)` and action triggers.

### 2. Mobile Canvas Scaler Settings
To ensure UI controls feel natural on any screen (from budget Android phones to tablets and widescreen monitors):
- **Canvas Render Mode:** `Screen Space - Overlay`
- **Canvas Scaler UI Scale Mode:** `Scale With Screen Size`
- **Reference Resolution:** `1920 x 1080`
- **Screen Match Mode:** `Match Width Or Height`
- **Match:** `0.5`
- **Joystick Anchor:** Bottom-Left (`Anchors Min (0,0)`, `Max (0,0)`)
- **Action Buttons Anchor:** Bottom-Right (`Anchors Min (1,0)`, `Max (1,0)`)

### 3. Context-Sensitive Control Switching (`MobileControlsToggle.cs`)
- **On Foot:** Displays Virtual Joystick + Jump + Sprint + Enter Danfo buttons.
- **Driving Danfo:** Switches to Gas + Brake + Steer / Joystick + Handbrake Drift + Lagos Horn + Exit Vehicle buttons.

---

## 🚗 Danfo Vehicle Physics Configuration

1. **Center of Mass Stabilization:**
   Tall minibuses (Danfo) easily tip over on sharp turns if standard colliders are used. In `CarController.cs`:
   ```csharp
   rb.centerOfMass += new Vector3(0f, -0.6f, 0f);
   ```
2. **WheelColliders Setup:**
   - 4 `WheelCollider` instances attached to empty child objects aligned with wheel hubs.
   - Forward and Sideways Friction: Extremum Slip `0.2`, Extremum Value `1.0`, Asymptote Slip `0.5`, Asymptote Value `0.75`.
   - Suspension Distance: `0.2m`, Spring: `35000`, Damper: `4500`.

---

## ⚡ 6-Month Cross-Platform Roadmap

| Milestone | Focus | Key Deliverables |
| :--- | :--- | :--- |
| **Month 1** | Input & Third-Person Locomotion | Unified InputManager, Virtual Joystick, CharacterController, Cinemachine follow camera |
| **Month 2** | Vehicle Physics | WheelColliders, Danfo handling, entry/exit transitions, mobile driving UI |
| **Month 3** | World Slice Blockout | 1 Lagos Street (kiosks, danfo stops), low-poly props, NavMesh pedestrians, budget Android test |
| **Month 4** | Gameplay Loop & Economy | Passenger pick-up/drop-off missions, Naira fare economy, JSON save/load system |
| **Month 5** | Lagos Ambience & Polish | Afrobeats in-game radio, traffic AI, day/night lighting, street banter |
| **Month 6+** | Optimization & Multiplatform Ship | Occlusion culling, texture atlasing, LOD groups, Android build (Google Play) & PC (itch.io) |

---

## 📱 Mobile vs PC Optimization Guidelines

1. **Draw Calls & Batches:** Keep draw calls under **120** on mobile using Texture Atlasing and Static Batching.
2. **Lighting:** Bake Global Illumination (Lightmaps) for mobile; use realtime directional shadow cascades on PC.
3. **World Streaming:** Divide the Lagos metropolis into grid chunks (~200m x 200m) loaded asynchronously via `Addressables` or additive scene loading.
