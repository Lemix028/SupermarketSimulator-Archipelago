# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [0.2.1] - 2026-07-19

### APWorld (Python / Archipelago Generator)

#### Added
- Exclude Licenses Option: Added `exclude_licenses` setting to exclude specified product licenses by name in YAML from the "All Licenses" victory goal. Excluded licenses are dynamically categorized as `useful` items instead of `progression` items.
- Starting Cash Option: Added `starting_cash` setting to customize initial money in new save games (range: $10 to $10,000, default: $50).
- Free Customizables Option: Added `free_customizables` toggle option to make all visual upgrades free.
- Checkout Income Multiplier Option: Added `checkout_income_multiplier` setting to multiply customer payments at checkout registers (range: 10% to 1000%, default: 100%).

### Client Mod (C# / BepInEx 6 IL2CPP)

#### Added
- Free Customizables Support: Implemented memory mutation and UI hooks to make wall paint, floor tiles, store rename, door placement, and entrance upgrades cost $0 when the option is enabled.
- Starting Cash Support: Set starting money on new game creations based on server slot data.
- Checkout Income Multiplier Support: Prefixed checkout transactions to apply the custom income multiplier when customers pay.
- Release Notification Suppression: Suppresses individual item popup notifications during item releases and shows a single goal completed notification.

#### Fixed
- Licenses Victory Goal Bug: Fixed early victory triggers. The client now checks the full required list of active licenses sent from the server rather than just received items.
- Thread Safety (AccessViolationException) Crash: Wrapped all UI notifications in the Unity main thread dispatcher to prevent crashes during rapid item receipt (e.g. releases).
- German Comment Translation: All remaining German source comments and log text have been translated to English.

## [0.2.0] - 2026-07-18 — Initial Public Release

### APWorld (Python / Archipelago Generator)

#### Added
- Initial public release of the Supermarket Simulator APWorld.
- Three goal options: **Level** (reach max store level), **Days** (complete max days), **All Licenses** (collect every product license in the pool).
- Dynamic location generation based on player YAML settings:
  - Store Level milestone checks (configurable interval, up to Level 200).
  - Days Completed milestone checks (configurable interval, up to Day 1000).
  - Storage Room Upgrade checks (optional, 20 upgrades).
  - Section Room purchase checks (optional, 32 sections).
  - Money Milestone checks (optional, configurable interval up to 500,000 $).
- Full DLC compatibility: Bakery, Ice Cream, Vending Machine, Electronics, Hardware, Clothing, Essentials — each optionally injectable into the item pool.
- Price Randomization option: Disabled / Balanced (+/-20%) / Chaotic (+/-50%).
- Filler items: Money Boost, XP Boost, Black Friday Event (with configurable weights).
- Trap items: Tax Audit, Dust Storm, Power Outage, Trash Flood, Expired Products, Robbery (with configurable frequency and disable list).
- Starting inventory configuration for licenses, vehicles, and furniture.
- Granular lock toggles: Furniture Locks, Vehicle Locks, Storage Locks, Section Locations, Loan Locks.
- Three preset configurations on the options page: **Easy**, **Normal**, **Hard**.
- Proper Victory event item/location system with per-goal access rules.

### Client Mod (C# / BepInEx 6 IL2CPP)

#### Added
- Initial public release of the BepInEx 6 (IL2CPP) client mod.
- In-game Archipelago Login Menu: Clicking the connect button opens UI window where players can enter and modify their credentials.
- Config Persistence: Credentials entered in-game are automatically saved to `com.lemix028.supermarketsimulator.archipelago.cfg` on click.
- Disconnect Option: The main menu button acts as a toggle: opens the login menu when disconnected, and disconnects the active session when clicked while connected.
- Copy-Paste (Ctrl+V) Support**: Easily paste long server URLs or passwords directly from the Windows clipboard.
- Connection HUD in the top-left corner showing server status and goal progress.
- In-game notification popups for received items, sent checks, and traps.
- Automatic reconnection with exponential backoff (up to 5 attempts) when the connection is lost.
- Full slot data parsing from the server to apply all player options in-game.
- Harmony patches for: Licenses, Sections, Furniture, Vehicles, Staff (Cashiers, Restockers, Janitors, Security, Customer Helpers, Bakers, Ice Cream Helpers), Storage Upgrades, Loan Authorizations, Vending Machine Slots.
- Price Randomization system applied to all local market, computer shop, and wholesale prices.
- Trap execution system for all six trap types.
- Filler execution system: Money Boost, XP Boost, Black Friday Event.
- Save-file-aware history manager to prevent duplicate trap/filler execution across sessions.

