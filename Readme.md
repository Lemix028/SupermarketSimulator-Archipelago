
# Supermarket Simulator - Archipelago Multiworld Client

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Game Version](https://img.shields.io/badge/Game%20Version-1.4.2-blue.svg)](https://store.steampowered.com/app/2670630/Supermarket_Simulator/)
[![Framework](https://img.shields.io/badge/Framework-BepInEx%206%20(IL2CPP)-orange.svg)](https://github.com/BepInEx/BepInEx)

This is a **C# Client Mod** for integrating *Supermarket Simulator* into the **Archipelago Multiworld Network**. 

## Features

### Tracked Locations
* Store levels completed at configured intervals
* Days completed at configured intervals
* Storage room upgrades
* Money milestones achieved

### Randomized Items
* Product licenses
* Store expansion sections
* Hireable staff members (cashiers, restockers, janitors, security, customer helpers)
* Furniture and displays (shelves, fridges, freezers and co.)
* Vehicles
* Loan authorizations

### Fillers and Traps
* Boosts (Money, XP, Black Friday event)
* Negative events (tax audits, power outages, dust storms, trash floods, expired products, robberies)

### Others
* Connection HUD showing server status in the top-left corner
* Ingame Notifications for Items and Locations

## 🛠️ Installation

### 1. Install BepInEx 6
1. Download **BepInEx 6 (Unity IL2CPP for Windows x64)** from the official repository.
2. Extract all files from the BepInEx `.zip` archive directly into your main **Supermarket Simulator** game folder (where `Supermarket Simulator.exe` is located).

### 2. Install the Mod
1. Download the [latest release](https://github.com/Lemix028/SupermarketSimulator-ArchipelagoMod/releases).
2. Open your game's directory and navigate to `BepInEx/plugins/`. *(If the folder doesn't exist, run the game once to let BepInEx generate it, or create it manually)*.
3. Extract `SupermarketArchipelago.dll` and its required dependency `.dll` files into the `plugins/` folder.

---

## ⚙️ Configuration

1. Launch **Supermarket Simulator** once to let BepInEx generate the default configuration file, then close the game.
2. Navigate to `BepInEx/config/` and open `com.lemix.supermarket.archipelago.cfg` in a text editor (like Notepad).
3. Fill in your Archipelago connection details:

```ini
[Archipelago]

## The connection address of the Archipelago server (IP:Port)
# Setting type: String
# Default value: archipelago.gg:38281
ServerAddress = archipelago.gg:38281

## Your exact slot name defined in your player YAML configuration
# Setting type: String
# Default value: Player
SlotName = Lemix

## The room password (leave empty if the host did not set one)
# Setting type: String
# Default value: 
Password =

```
4.  Save the file, launch the game, click on AP Server Connect Button in the Main Menu, and you will automatically connect. An active **Connection HUD** in the top-left corner of the screen will show your status.
    

## 💻 Compiling From Source

If you want to modify or compile this mod yourself:

### Prerequisites
  
-   **.NET 6.0 SDK**
    
-   NuGet package `Archipelago.MultiClient.Net`.
    

### Setup Reference DLLs

Since Supermarket Simulator uses the Unity IL2CPP backend, you need the stripped assembly DLLs for compiling. You can find all DLLs in `BepInEx/interop/` and `BepInEx/core/`.
    

## 📄 License

This project is licensed under the **MIT License**.
