# Bagh-Bandi (Multiplayer Game Prototype)

## 📌 Overview
Bagh-Bandi is a strategic two-player game inspired by the traditional board game, where one player controls the **Predator (Tiger)** and the other controls the **Prey (Goats)**. This project serves as a prototype for implementing **turn-based multiplayer mechanics** using **Unity and Netcode for GameObjects**.

## 🎮 Gameplay
- The **Predator** aims to capture the **Prey** by jumping over them.
- The **Prey** must trap the **Predator** by blocking its movement.
- Players take turns moving their respective pieces.
- The game ends when all prey are captured, or the predator can no longer move.

## 🔥 Features
- **Turn-based System**: Players take turns controlling their respective pieces.
- **Visual Indicators**: Highlights valid moves for each turn.
- **Score Tracking**: Displays remaining prey and capture count.
- **Event-driven UI**: Uses an event system to update UI dynamically.
- **Game Over Condition**: Determines the winner based on game state.
- **Multiplayer Ready (Upcoming)**: Planned online multiplayer support.

## 🚀 Technology Stack
- **Game Engine**: Unity 6
- **Programming Language**: C#
- **Networking**: Netcode for GameObjects (Prototype Phase)
- **UI System**: TextMeshPro, Unity UI
- **Version Control**: Git & GitHub

## 📂 Project Structure
```
📦 Bagh-Bandi
 ┣ 📂 Assets
 ┃ ┣ 📂 Scripts
 ┃ ┃ ┣ 🎮 GameManager.cs  // Handles game state & turns
 ┃ ┃ ┣ 🎮 InputHandler.cs  // Manages player input, New Input system
 ┃ ┃ ┣ 🎮 GameLogic.cs  // Handles game logic
 ┃ ┃ ┣ 🎮 PlayerController.cs  // Handles player movement & actions
 ┃ ┃ ┣ 🎮 GameUiManager.cs  // Manages UI elements & events
 ┃ ┃ ┣ 🎮 Marker.cs  // Manages the board layout and nodes
 ┃ ┣ 📂 Sprites
 ┃ ┣ 📂 Prefabs
 ┣ 📜 README.md
```

## 🎯 Roadmap
- [x] Implement core mechanics & turn-based system
- [x] Add UI & event-driven updates
- [ ] Integrate online multiplayer (Netcode for gameobject)
- [ ] Add AI for single-player mode
- [ ] Polish UI & animations

## 🤝 Contributing
Feel free to fork this repo and contribute! Open an issue if you have suggestions or encounter bugs.
---
🚀 *A strategic game that challenges your tactical skills!*

