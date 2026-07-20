
# Supermarket Simulator - Archipelago Multiworld Client Mod

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Game Version](https://img.shields.io/badge/Game%20Version-1.4.2-blue.svg)](https://store.steampowered.com/app/2670630/Supermarket_Simulator/)
[![Framework](https://img.shields.io/badge/Framework-BepInEx%206%20(IL2CPP)-orange.svg)](https://github.com/BepInEx/BepInEx)

This is a **C# Client Mod** for integrating *Supermarket Simulator* into the **Archipelago Multiworld Network**.

The repository also includes the Archipelago World definition (APWorld) located in the `apworld` directory.

## Features

### Tracked Locations
* Store levels completed at configured intervals
* Days completed at configured intervals
* Section and Storage room upgrades
* Money milestones achieved

### Randomized Items
* Product licenses
* Store expansion sections
* Hireable staff members (cashiers, restockers, janitors, security, customer helpers)
* Furniture and displays (shelves, fridges, freezers and co.)
* Vending machine slots
* Vehicles
* Loan authorizations

### Fillers and Traps
* Boosts (Money, XP, Black Friday event)
* Negative events (tax audits, power outages, dust storms, trash floods, expired products, robberies)

### Mechanics & Customization
* **Full DLC Compatibility**: Completely compatible with all available game DLCs (Bakery, Ice Cream, Vending, Electronics, Hardware, Clothing, Essentials), which can be dynamically enabled via your Archipelago options.
* **Price Randomization**: Optional setting to completely randomize purchase prices for all products bought via the local market, the computer shop, or wholesale orders.
* **Exclude Licenses**: Ability to exclude specific licenses by name from the "All Licenses" victory goal.
* **Checkout Income Multiplier**: Option to multiply customer payments at checkout registers (10% to 1000%).
* **Starting Cash**: Option to customize starting money ($10 to $10,000) for new save games.
* **Free Customizables**: Option to make all store visual customizations (wall paint, floor tiles, rename, door placement, entrance variants) cost $0.
* **Connection HUD**: Ingame HUD showing server status in the top-left corner.
* **Notifications**: Integrated popup system for incoming items and sent locations.

## Installation

### 1. Install BepInEx 6
1. Download **BepInEx 6 (Unity IL2CPP for Windows x64)** from the [official repository](https://github.com/BepInEx/BepInEx). Currently, the **IL2CPP** version is only available through the [Bleeding Edge Releases](https://builds.bepinex.dev/projects/bepinex_be). Not the **Mono** version!
2. Extract all files from the BepInEx `.zip` archive directly into your main **Supermarket Simulator** game folder (where `Supermarket Simulator.exe` is located).

### 2. Install the Mod and APWorld
1. Download the latest release from the [GitHub Releases page](https://github.com/Lemix028/SupermarketSimulator-Archipelago/releases). Both the client mod (SupermarketArchipelago_Client_vX.X.X.zip ) and the Archipelago world package (.apworld) are available there.
2. Open your game's directory and navigate to `BepInEx/plugins/`. (If the folder doesn't exist, run the game once to let BepInEx generate it, or create it manually).
3. Extract `SupermarketSimArchipelago.dll` and its required dependency `.dll` files into the `plugins/` folder.
4. Place the downloaded `.apworld` file in your Archipelago custom worlds directory: `C:\ProgramData\Archipelago\custom_worlds`

---

## Connection & Setup

You can set up your Archipelago connection credentials directly in-game or via the configuration file.

### 1. Connecting In-Game (Recommended)
1. Launch **Supermarket Simulator**.
2. Click the **AP Server Connect** button in the main menu to open the connection details window.
3. Enter your connection details:
   * **Server Address**: The address of your Archipelago server (e.g., `archipelago.gg:38281`). You can use **Ctrl+V** to paste directly from your clipboard.
   * **Slot Name**: Your player slot name as configured in your YAML.
   * **Password**: The password for the room (leave blank if none).
4. Click **Connect & Save**. The credentials will be saved automatically, and the mod will attempt to connect.
5. Once connected, your status and goal progress will be displayed in the **Connection HUD** in the top-left corner of the screen. You can now load your save game or start a new one!

### 2. Alternative: Editing the Config File
If you prefer, you can pre-configure your credentials before launching the game:
1. Launch the game once so BepInEx can generate the config file, then close the game.
2. Open `<GameDir>/BepInEx/config/com.lemix028.supermarketsimulator.archipelago.cfg` in a text editor.
3. Fill in your server details:
   ```ini
   [Archipelago]
   ServerAddress = archipelago.gg:38281
   SlotName = Lemix028
   Password = 
   ```
4. Save the file and start the game. Click **AP Server Connect** in the Main Menu to open the login UI with these details pre-filled, then click **Connect** to establish the connection.
    

## Compiling From Source

If you want to modify or compile this mod yourself:

### Prerequisites
  
-   **.NET 6.0 SDK**
    
-   NuGet package `Archipelago.MultiClient.Net`.
    

### Setup Reference DLLs

Since Supermarket Simulator uses the Unity IL2CPP backend, you need the stripped assembly DLLs for compiling. You can find all DLLs in `BepInEx/interop/` and `BepInEx/core/`.
    

## License

This project is licensed under the MIT License - see the LICENSE file for details.


## Changelog

For a full version history, see the [GitHub Releases page](https://github.com/Lemix028/SupermarketSimulator-Archipelago/releases).

