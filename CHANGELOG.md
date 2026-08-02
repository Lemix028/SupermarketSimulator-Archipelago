# Changelog

## [0.3.0] - Unreleased

### APWorld (Python)

#### Added
- License Purchase Location Checks: Purchasing a received product license at the in-game computer now acts as a location check (`Purchase License X`). Starting licenses and excluded licenses do not generate purchase checks.
- Customer Checkout Location Checks: Added `customer_checkout_locations` (0–10000, default 100) and `customer_checkout_chance` (1–100%, default 5%) YAML options. Completing customer checkouts probabilistically triggers location checks.
- YAML Options Guidance: Added recommendation docstrings and hints to options in `options.py`.

#### Changed
- Default Victory Goal: Changed default goal in `options.py` to **`All Licenses`** (Goal 2).
- Store Level 1 Excluded: Removed `Store Level 1` location check since players begin the game at Store Level 1.
- Extended Customer Checkout Range: Increased maximum `customer_checkout_locations` limit from 100 to 10,000 in `locations.py` and `options.py`.
- Renamed all Python files in the APWorld to lowercase/snake_case (`items.py`, `locations.py`, `options.py`, `rules.py`, `license_data.py`).

#### Fixed
- Furniture Item Classification: Updated `Speaker`, `Category Sign`, and `Trash Can` classification from `filler` to `useful` in `items.py`.
- Universal Tracker Crash (ZeroDivisionError): Populated `location_name_groups` with all location categories and corrected `Store Level` range starting at Level 2.
- Section Upgrade Unlocks: Prevented section upgrades from becoming unlocked while section locations are disabled.

### Client Mod (C# / BepInEx 6 IL2CPP)

#### Added
- Power Outage Manager: `PowerOutageManager.cs` to handle store light dimming and 60-second restoration in Unity's `Update()` loop.
- Notification Queue & Release Filter: Implemented in-game notification queue (`_queue`) in `NotificationManager.cs` with dynamic speedup, and ensured Goal Release displays exclusively `"Goal Completed! Releasing all items!"`.
- Single Self-Found Item Notification
- License Purchase Location Handler
- Customer Checkout Location Handler

#### Fixed
- Staff Hiring Locks: Added `SelectableInteractablePatch` to enforce `interactable = false` for all staff members whose Archipelago item has not been received, keeping hire buttons locked and unclickable.
- Store Level Goal Verification: Hooked goal verification into `SetStoreReady`, `CheckLevelChange`, `AddPoint`, `RefreshLevel`, and XP boosts so level goal completion is detected immediately.
- Money Boost Multiplier Fix: Prevented Money Boost filler items from being multiplied by `CheckoutIncomeMultiplier` or triggering false checkout location checks.
- Inactive UI Component Refresh: Updated `UIHelper` refresh methods (including `RefreshSectionUI`) to include inactive UI components (`includeInactive = true`), unlocking received growth sections immediately in the background.
- Day Completed Offset Bug: Fixed `DayCompletedProgressPatch` sending `Day 2 Completed` on finishing Day 1 by correcting `__instance.CurrentDay - 1` calculation.
- Section Purchase Money Check: Disabled section room upgrade purchase button when the player lacks sufficient money.

## [0.2.1] - 2026-07-19 — Initial Public Release

### APWorld (Python)

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

## [0.2.0] - 2026-07-18 

### APWorld (Python)

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

---

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
