# Troubleshooting Guide — Supermarket Simulator Archipelago Mod

This document covers the most common problems encountered when setting up or playing the Archipelago integration.

---

## The Mod Does Not Load / The Connect Button Is Missing

**Cause:** BepInEx is not installed correctly, or the mod DLL is in the wrong location.

**Solution:**
1. Verify that BepInEx 6 (**Unity IL2CPP** version) is installed — not BepInEx 5. The mod will not work with BepInEx 5.
2. The file SupermarketArchipelago.dll must be placed directly in <GameDir>/BepInEx/plugins/. There should be **no** subfolder.
3. The two dependency DLLs (Archipelago.MultiClient.Net.dll and Newtonsoft.Json.dll) must also be in the **same** plugins/ folder.
4. Start the game once after installing BepInEx to let it generate the required folders.
5. Check the BepInEx log file at <GameDir>/BepInEx/LogOutput.log for any errors.


---

## Received Items Are Not Being Applied In-Game

**Cause:** The store is not yet ready, or the mod has not synced state correctly.

**Solution:**
1. Items that arrive while the store is loading are queued automatically and applied once the world is loaded.
2. Make sure you are fully in the store (past the loading screen) before receiving items.
3. If items are missing after reconnecting, disconnect and reconnect to the server to trigger a full re-sync.
4. Check BepInEx/LogOutput.log for errors related to item processing.

---

## Location Checks Are Not Being Sent

**Cause:** The mod has lost the server connection, or the check was already sent in a previous session.

**Solution:**
1. The connection HUD in the top-left should show "Connected". If it shows disconnected, the mod will automatically attempt to reconnect. Wait a few seconds.
2. Checks already sent to the server will not be sent again (they are cached). This is intentional.
3. If the HUD shows connected but checks still are not sending, restart the game and reconnect.

---

## The Auto-Reconnect Is Failing

**Cause:** The server is offline, the port changed, or there is a network issue.

**Solution:**
1. The mod will attempt to reconnect up to **5 times** with increasing delays (1s, 2s, 4s, 8s, 16s).
2. If all attempts fail, you will see a log message: *"Reconnect failed after maximum attempts. Please reconnect manually."*
3. Use the in-game **AP Server Connect** button to reconnect manually.
4. Note: If the room was moved to a new port, you need to update the Server URL in the in-game login menu (or in the config file).

---

## Prices Are Not Being Randomized

**Cause:** Price Randomization is disabled in your YAML settings, or the slot data was not parsed correctly.

**Solution:**
1. Check your YAML to confirm price_randomization is not set to disabled.
2. Disconnect and reconnect to the server — slot data is only parsed on successful login.
3. Prices are applied to the in-game market **when the computer shop or market UI is opened**, not immediately on receipt of the option.

---

## DLC Items Are Not Appearing / Working

**Cause:** The DLC option is not enabled in your YAML, or the DLC is not installed.

**Solution:**
1. Check your YAML settings — each DLC must be explicitly added to the active_dlcs list.
2. Confirm the DLC is actually purchased and installed on Steam.
3. If a DLC item is in your inventory but the in-game feature is not unlocked, try reloading the store.

---

## BepInEx Log Location

The main log file is located at:
`
<Steam Game Directory>/BepInEx/LogOutput.log
`
Always share this log when reporting bugs — it contains all error messages from the mod.

---

## Reporting Bugs

If your problem is not listed here, please open an issue on GitHub:
[https://github.com/Lemix028/SupermarketSimulator-Archipelago/issues](https://github.com/Lemix028/SupermarketSimulator-Archipelago/issues)

Please include:
- Your BepInEx/LogOutput.log
- Your YAML file (remove personal info if needed)
- The Archipelago server version you are connecting to
- Your game version (visible in the Main Menu bottom right)
