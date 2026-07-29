# Troubleshooting Guide — Supermarket Simulator Archipelago Mod

This document covers the most common problems encountered when setting up or playing the Archipelago integration.

---

## 📑 Table of Contents

- [Red Errors Appearing in the Console](#red-errors-appearing-in-the-console)
- [How to Temporarily Disable BepInEx to Play Vanilla](#how-to-temporarily-disable-bepinex-to-play-vanilla)
- [The Mod Does Not Load / The Connect Button Is Missing](#the-mod-does-not-load--the-connect-button-is-missing)
- [Received Items Are Not Being Applied In-Game](#received-items-are-not-being-applied-in-game)
- [Location Checks Are Not Being Sent](#location-checks-are-not-being-sent)
- [The Auto-Reconnect Is Failing](#the-auto-reconnect-is-failing)
- [Prices Are Not Being Randomized](#prices-are-not-being-randomized)
- [DLC Items Are Not Appearing / Working](#dlc-items-are-not-appearing--working)
- [BepInEx Log Location](#bepinex-log-location)
- [Reporting Bugs](#reporting-bugs)

---

## Red Errors Appearing in the Console

**Cause / Explanation:** This is **completely normal**. The base game natively throws console errors that have nothing to do with Archipelago or this mod. This is especially true for messages starting with `[Error / Warning / Info : Unity]`.

Only log entries with the following prefixes are relevant to the mod:

* `[Error / Warning / Info : Supermarket Simulator Archipelago]`
* `[Error / Warning / Info : BepInEx]`


---

## How to Temporarily Disable BepInEx to Play Vanilla

**Solution:** If you want to launch the normal game without mods, you don't need to uninstall everything. Simply rename the BepInEx core file:

1. Open your game directory: `...\Supermarket Simulator\`
2. Find the file `winhttp.dll`
3. Rename it to `winhttp.dll.disabled` *(or simply add `.bak` at the end)*

To reactivate your mods later, just rename the file back to `winhttp.dll`.

> [!CAUTION]
> **Do NOT load your Archipelago save file in vanilla mode!**
> Opening a modded save without the Archipelago mod active can permanently break your progress, corrupt item data, or cause missing items. Always start a new save game or use a completely separate save file reserved for unmodded play.


---

## The Mod Does Not Load / The Connect Button Is Missing

**Cause:** BepInEx is not installed correctly, or the mod DLL is in the wrong location.

**Solution:**
1. Verify that **BepInEx 6 (Unity IL2CPP version)** is installed — *not* BepInEx 5! The mod will not work with BepInEx 5 or the Unity Mono version.
2. The file `SupermarketArchipelago.dll` must be placed directly in `<GameDir>/BepInEx/plugins/`. There should be **no subfolder**.
3. The two dependency DLLs (`Archipelago.MultiClient.Net.dll` and `Newtonsoft.Json.dll`) must also be in the same `plugins/` folder.
4. Start the game once after installing BepInEx to let it generate the required folders.
5. Check the BepInEx log file at `<GameDir>/BepInEx/LogOutput.log` for any errors.


---

## Received Items Are Not Being Applied In-Game

**Cause:** The store is not yet ready, or the mod has not synced state correctly.

**Solution:**
1. Items that arrive while the store is loading are queued automatically and applied once the world is loaded.
2. If items are missing after reconnecting, disconnect and reconnect to the server to trigger a full re-sync.
3. Server items are only sent once per seed. To prevent items from triggering multiple times, their state is saved locally in the cache file (`...\Supermarket Simulator\BepInEx\cache`).
4. Check `BepInEx/LogOutput.log` for errors related to item processing.


---

## Location Checks Are Not Being Sent

**Cause:** The mod has lost the server connection, or the check was already sent in a previous session.

**Solution:**
1. The connection HUD in the top-left should show **"Connected"**. If it shows disconnected, the mod will automatically attempt to reconnect. Otherwise, save, return to the main menu, and try to reconnect.
2. If the HUD shows connected but checks still are not sending, restart the game and reconnect.
3. Check the BepInEx log file at `<GameDir>/BepInEx/LogOutput.log` for any errors.


---

## The Auto-Reconnect Is Failing

**Cause:** The server is offline, the port changed, or there is a network issue.

**Solution:**
1. The mod will attempt to reconnect up to **5 times** with increasing delays (1s, 2s, 4s, 8s, 16s).
2. If all attempts fail, you will see a log message: *"Reconnect failed after maximum attempts. Please reconnect manually."*
3. Use the in-game **AP Server Connect** button to reconnect manually.
4. **Note:** If the room was moved to a new port, you need to update the Server URL in the in-game login menu.
5. Check the BepInEx log file at `<GameDir>/BepInEx/LogOutput.log` for any errors.


---

## Prices Are Not Being Randomized

**Cause:** Price Randomization is disabled in your YAML settings, or the slot data was not parsed correctly.

**Solution:**
1. Check your YAML to confirm `price_randomization` is not set to disabled.
2. Disconnect and reconnect to the server — slot data is only parsed on successful login.
3. Check the BepInEx log file at `<GameDir>/BepInEx/LogOutput.log` for any errors.


---

## DLC Items Are Not Appearing / Working

**Cause:** The DLC option is not enabled in your YAML, or the DLC is not installed.

**Solution:**
1. Check your YAML settings — each DLC must be explicitly added to the `active_dlcs` list.
2. Confirm the DLC is actually purchased and installed on Steam.
3. Check the BepInEx log file at `<GameDir>/BepInEx/LogOutput.log` for any errors.


---

## BepInEx Log Location

The main log file is located at:
```text
<Steam Directory Game>/BepInEx/LogOutput.log

```

Always share this log when reporting bugs — it contains all error messages from the mod.


---

## Reporting Bugs

If your problem is not listed here, please report it via **GitHub** or **Discord**:

* **GitHub Issues:** [Create an issue on GitHub](https://github.com/Lemix028/SupermarketSimulator-Archipelago/issues)
* **Discord Thread:** [Join the discussion on Discord](https://discord.com/channels/731205301247803413/1290960179407360030) *(Future Games / Archipelago)*

Please include the following details in your report:

* Your `BepInEx/LogOutput.log` file
* Your YAML file *(remove personal info if needed)*
* The Archipelago server version you are connecting to *(e.g. 0.6.7)*
* Your game version *(visible in the bottom-right of the Main Menu, e.g. 1.5.2)*


```
