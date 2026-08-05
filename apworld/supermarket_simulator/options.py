from Options import Range, Toggle, DefaultOnToggle, OptionList, Choice, PerGameCommonOptions, OptionSet
from dataclasses import dataclass
from .items import ALL_LICENSES, ALL_FURNITURE, ALL_VEHICLES, ALL_TRAPS, dlc_licenses
from .license_data import LICENSE_DETAILS

class ActiveDLCs(OptionSet):
    """Select which DLCs are active. 
    Enabling a DLC dynamically injects its licenses and items into the item pool.
    Options: dlc_ice_cream, dlc_bakery, dlc_vending, dlc_essentials, dlc_hardware, dlc_electronics, dlc_clothing
    Recommended: Enable all DLCs (only if you own them)."""
    display_name = "Active DLCs"
    valid_keys = {
        "dlc_ice_cream",
        "dlc_bakery",
        "dlc_vending",
        "dlc_essentials",
        "dlc_hardware",
        "dlc_electronics",
        "dlc_clothing",
    }
    default = set() 

class Goal(Choice):
    """The victory condition for your Archipelago round.
    Level: Reach the configured maximum store level.
    Days: Complete the configured maximum number of operating days.
    All Licenses: Collect every product license in the item pool (base game + active DLCs).
    Recommended: All Licenses."""
    display_name = "Goal"
    option_level = 0
    option_days = 1
    option_all_licenses = 2
    default = 2

class PriceRandomization(Choice):
    """Randomizes the prices of items in the store. This affects the price you pay for items, not the selling price of your products!.
    Disabled: Standard prices.
    Balanced: Prices vary by up to +/- 20%.
    Chaotic: Prices vary by up to +/- 50%. Margins remain stable.
    Recommended: Balanced."""
    display_name = "Price Randomization"
    option_disabled = 0
    option_balanced = 1
    option_chaotic = 2
    default = 1

class MaxStoreLevel(Range):
    """The maximum store level that will contain locations (checks).
       Recommended: Depends how long you want to play."""
    display_name = "Max Store Level"
    range_start = 5
    range_end = 100
    default = 50

class StoreLevelInterval(Range):
    """How many levels should be between individual checks. E.g., 2 means Level 2, 4, 6...
       Recommended: 1"""
    display_name = "Store Level Interval"
    range_start = 1
    range_end = 10
    default = 1

    def verify(self, world, player_name, plando_options):
        world_options = getattr(world, "options", None) if world else None
        max_lvl = getattr(world_options, "max_store_level", None) if world_options else None
        if max_lvl and self.value > max_lvl.value:
            self.value = max_lvl.value
        super().verify(world, player_name, plando_options)

class MaxDaysCompleted(Range):
    """The maximum number of completed days that contain checks.
    Recommended: 100"""
    display_name = "Max Days Completed"
    range_start = 5
    range_end = 200
    default = 100

class DaysCompletedInterval(Range):
    """How many completed days should be between checks. E.g., 5 means Day 5, 10, 15...
       Recommended: 1"""
    display_name = "Days Completed Interval"
    range_start = 1
    range_end = 20
    default = 1

    def verify(self, world, player_name, plando_options):
        world_options = getattr(world, "options", None) if world else None
        max_days = getattr(world_options, "max_days_completed", None) if world_options else None
        if max_days and self.value > max_days.value:
            self.value = max_days.value
        super().verify(world, player_name, plando_options)

class EnableSectionLocations(DefaultOnToggle):
    """If enabled, buy section room locations can be generated.
    Recommended: Enabled."""
    display_name = "Enable Section Locations"

class EnableFurnitureLocks(DefaultOnToggle):
    """If enabled, furniture (fridges, shelves) is locked behind AP items.
    If disabled, they are immediately available and removed from the item pool.
    Recommended: Enabled."""
    display_name = "Enable Furniture Locks"

class EnableVehicleLocks(DefaultOnToggle):
    """If enabled, vehicles are locked behind AP items.
    If disabled, they are immediately available.
    Recommended: Enabled."""
    display_name = "Enable Vehicle Locks"

class EnableStorageLocks(DefaultOnToggle):
    """If enabled, storage room upgrades are locked behind AP items and also act as checks.
    If disabled, they are immediately available and removed from the item pool.
    Recommended: Enabled."""
    display_name = "Enable Storage Locks"

class EnableLoanLocks(DefaultOnToggle):
    """If enabled, credit line licenses (1-6) are locked behind AP items.
    If disabled, they are immediately available and removed from the item pool.
    Recommended: Enabled."""
    display_name = "Enable Loan Locks"

# === STARTING ITEMS CONFIGURATIONS ===

class StartingLicenses(OptionSet):
    """Set of product licenses you want to start the game with (e.g., 'License 21'). License 21 needs to be included currently because the game sets it as a starting license.
    Recommended: Default except if you want to start with more licenses."""
    display_name = "Starting Licenses"
    valid_keys = set(ALL_LICENSES)
    default = {"License 21"}

    def verify(self, world, player_name, plando_options):
        world_options = getattr(world, "options", None) if world else None
        active_dlcs = getattr(world_options, "active_dlcs", None) if world_options else None
        active_dlc_keys = active_dlcs.value if active_dlcs else set()
        for item in self.value:
            for dlc_key, dlc_items in dlc_licenses.items():
                if item in dlc_items and dlc_key not in active_dlc_keys:
                    raise ValueError(f"Starting license '{item}' requires DLC '{dlc_key}', which is not enabled in active_dlcs for {player_name}'s slot.")
        super().verify(world, player_name, plando_options)

class StartingVehicles(OptionSet):
    """Set of vehicles you want to start the game with (e.g., 'Skateboard').
       Recommended: None except if you want to start easy."""
    display_name = "Starting Vehicles"
    valid_keys = set(ALL_VEHICLES)
    default = set()

class StartingFurniture(OptionSet):
    """Set of furniture items you want to start the game with.
       Default game starts with two normal shelves and a checkout counter.
       Recommended: None except if you like to have basic furniture to begin like half shelf and mini fridge."""
    display_name = "Starting Furniture"
    valid_keys = set(ALL_FURNITURE)
    default = set()

# === TRAPS & FILLERS ===

class EnableTraps(DefaultOnToggle):
    """If enabled, harmful trap items can be generated as fillers.
    Recommended: Enabled unless you want a more peaceful experience."""
    display_name = "Enable Traps"

class TrapFrequency(Range):
    """The percentage chance (0-100) that a filler item becomes a trap instead of a booster.
        Recommended: 20"""
    display_name = "Trap Frequency"
    range_start = 0
    range_end = 100
    default = 20

class DisabledTraps(OptionSet):
    """Set of traps that should be excluded from generation.
    Recommended: None unless you want to avoid specific traps."""
    display_name = "Disabled Traps"
    valid_keys = set(ALL_TRAPS)
    default = set()

class FillerMoneyWeight(Range):
    """Generation weight for money boosters.
    Recommended: 50"""
    display_name = "Filler Money Weight"
    range_start = 0
    range_end = 100
    default = 50

class FillerXPWeight(Range):
    """Generation weight for store XP boosters.
    Recommended: 50"""
    display_name = "Filler XP Weight"
    range_start = 0
    range_end = 100
    default = 50

class EnableBlackfridayEvents(DefaultOnToggle):
    """If enabled, Black Friday events can be generated as fillers.
    Recommended: Enabled"""
    display_name = "Enable Black Friday Events"

class FillerBlackfridayWeight(Range):
    """Generation weight for Black Friday events.
    Recommended: 10"""
    display_name = "Filler Black Friday Weight"
    range_start = 0
    range_end = 100
    default = 10

class EnableMoneyMilestones(DefaultOnToggle):
    """If enabled, reaching specific money milestones unlocks Archipelago checks. You must hold this amount of money, not just earn it total.
    Recommended: Enabled"""
    display_name = "Enable Money Milestones"

class MaxMoneyMilestone(Range):
    """The maximum amount of money that includes checks.
    Recommended: 25000 because the game is designed to use your money to unlock checks and progress. Setting this too high can make the checks difficult to achieve."""
    display_name = "Max Money Milestone"
    range_start = 5000
    range_end = 500000
    default = 25000
    step = 5000

class MoneyMilestoneInterval(Range):
    """The interval between money checks (e.g., every $5000).
    Recommended: 2000-5000"""
    display_name = "Money Milestone Interval"
    range_start = 1000
    range_end = 100000
    default = 5000
    step = 1000

    def verify(self, world, player_name, plando_options):
        world_options = getattr(world, "options", None) if world else None
        max_money = getattr(world_options, "max_money_milestone", None) if world_options else None
        if max_money and self.value > max_money.value:
            self.value = max_money.value
        super().verify(world, player_name, plando_options)

class VendingMachineSlots(Range):
    """The number of Vending Machine Slot licenses placed in the item pool.
    Each item allows buying an additional vending machine slot in the computer.
    dlc_vending must be enabled for this option to have any effect.
    Recommended: Set it to the number of vending machine slots you want to be able to buy in the game."""
    display_name = "Vending Machine Slots"
    range_start = 0
    range_end = 20
    default = 5

class ExcludeLicenses(OptionSet):
    """List of product licenses to exclude from the 'All Licenses' victory goal.
    These licenses will still be in the item pool and function normally, but they will not be required to be collected or bought to win the game.
    Recommended: None unless you want make the game quicker or easier by excluding some licenses from the goal."""
    display_name = "Exclude Licenses"
    valid_keys = set(ALL_LICENSES)
    default = set()

    def verify(self, world, player_name, plando_options):
        world_options = getattr(world, "options", None) if world else None
        active_dlcs = getattr(world_options, "active_dlcs", None) if world_options else None
        active_dlc_keys = active_dlcs.value if active_dlcs else set()
        active_licenses = set()
        for item_name in ALL_LICENSES:
            is_dlc = False
            for dlc_key, licenses_dict in dlc_licenses.items():
                if item_name in licenses_dict:
                    is_dlc = True
                    if dlc_key in active_dlc_keys:
                        active_licenses.add(item_name)
                    break
            if not is_dlc:
                active_licenses.add(item_name)

        goal = getattr(world_options, "goal", None) if world_options else None
        if goal and goal.value == 2:  # option_all_licenses
            remaining = active_licenses - self.value
            if not remaining:
                raise ValueError(f"All active product licenses are excluded for {player_name}'s slot while Goal is set to 'All Licenses'.")
        super().verify(world, player_name, plando_options)

class CheckoutIncomeMultiplier(Range):
    """Multiplies the payout received at the cash register/self-checkout when customers pay.
    Represented in percentage (100 = 1.0x, 120 = 1.2x, 80 = 0.8x).
    Recommended: 100 (1.0x) but you can increase it to make the game easier or decrease it to make the game harder."""
    display_name = "Checkout Income Multiplier"
    range_start = 10
    range_end = 1000
    default = 100


class StartingCash(Range):
    """The cash the player starts with in a new game. Default game starts with $50.
    Recommended: 50 (Game default) but you can increase it to make the game easier."""
    display_name = "Starting Cash"
    range_start = 25
    range_end = 10000
    default = 50


class FreeCustomizables(Toggle):
    """If enabled, all store customization options (wall paint, floor tiles, store rename, door placement, entrance variants) cost $0.
       Since these are normally cosmetic-only, this option does not affect the game balance.
       Recommended: Your preference, but it can make the game more fun to customize your store without worrying about money."""
    display_name = "Free Customizables"
    default = 1

class CustomerCheckoutLocations(Range):
    """Number of Customer Checkout location checks generated. Every time a customer checks out, there is a chance to trigger a check at one of these locations.
    Recommended: 200 (Depends on how long you want to play.)"""
    display_name = "Customer Checkout Locations"
    range_start = 0
    range_end = 10000
    default = 200

class CustomerCheckoutChance(Range):
    """Percentage chance (40-100%) per checked-out customer to trigger a Customer Checkout location check.
    Recommended: 50 (Depends on a mix of how much Location you have and how long you want to play.)"""
    display_name = "Customer Checkout Chance"
    range_start = 40
    range_end = 100
    default = 50

class LocalCheckoutFill(Range):
    """Percentage of Customer Checkout locations pre-filled with local filler/trap items before global multiworld placement.

    Host Thresholds (enforced unless server host permits lower minimums):
    - Up to 500 checks: minimum 55% local fill
    - 501 to 2000 checks: minimum 90% local fill
    - Over 2000 checks: minimum 97% local fill
    - Maximum global multiworld checkout items: 400 max cap
    
    Recommended: 60"""
    display_name = "Local Checkout Fill Percentage"
    range_start = 0
    range_end = 100
    default = 60

item_to_dlcs = {}
# Required DLCs for each item
for dlc_key, dlc_items in dlc_licenses.items():
    for item_name in dlc_items.keys():
        if item_name not in item_to_dlcs:
            item_to_dlcs[item_name] = []
        item_to_dlcs[item_name].append(dlc_key)

# Mutliple DLCs can provide the same item, so store a list of DLCs for each item
def format_with_dlc_and_details(item_name: str) -> str:
    extra_info = []
    
    # Add DLC requirement info if the item is from a DLC
    if item_name in item_to_dlcs:
        extra_info.append(f"Requires {' or '.join(item_to_dlcs[item_name])}")
    
    # Add license details if the item is a license
    if item_name in LICENSE_DETAILS:
        details = LICENSE_DETAILS[item_name]
        extra_info.append(f"{details['products']} (Store Lvl: {details['level']}, Cost: ${details['cost']})")
    
    if extra_info:
        return f"{item_name} -> {', '.join(extra_info)}"
    return item_name

StartingLicenses.__doc__ += "\nAvailable options:\n- " + "\n- ".join(format_with_dlc_and_details(name) for name in ALL_LICENSES)
StartingFurniture.__doc__ += "\nAvailable options:\n- " + "\n- ".join(format_with_dlc_and_details(name) for name in ALL_FURNITURE)
StartingVehicles.__doc__ += "\nAvailable options:\n- " + "\n- ".join(format_with_dlc_and_details(name) for name in ALL_VEHICLES)
DisabledTraps.__doc__ += "\nAvailable options:\n- " + "\n- ".join(format_with_dlc_and_details(name) for name in ALL_TRAPS)
ExcludeLicenses.__doc__ += "\nAvailable options:\n- " + "\n- ".join(format_with_dlc_and_details(name) for name in ALL_LICENSES)




@dataclass
class SupermarketOptions(PerGameCommonOptions):
    active_dlcs: ActiveDLCs
    goal: Goal
    price_randomization: PriceRandomization
    max_store_level: MaxStoreLevel
    store_level_interval: StoreLevelInterval
    max_days_completed: MaxDaysCompleted
    days_completed_interval: DaysCompletedInterval
    enable_furniture_locks: EnableFurnitureLocks
    enable_vehicle_locks: EnableVehicleLocks
    enable_storage_locks: EnableStorageLocks  
    enable_section_locations: EnableSectionLocations
    enable_loan_locks: EnableLoanLocks        
    starting_licenses: StartingLicenses
    starting_vehicles: StartingVehicles
    starting_furniture: StartingFurniture
    exclude_licenses: ExcludeLicenses
    enable_traps: EnableTraps
    trap_frequency: TrapFrequency
    disabled_traps: DisabledTraps
    filler_money_weight: FillerMoneyWeight
    filler_xp_weight: FillerXPWeight
    enable_blackfriday_events: EnableBlackfridayEvents
    filler_blackfriday_weight: FillerBlackfridayWeight
    enable_money_milestones: EnableMoneyMilestones
    max_money_milestone: MaxMoneyMilestone
    money_milestone_interval: MoneyMilestoneInterval
    vending_machine_slots: VendingMachineSlots
    checkout_income_multiplier: CheckoutIncomeMultiplier
    starting_cash: StartingCash
    free_customizables: FreeCustomizables
    customer_checkout_locations: CustomerCheckoutLocations
    customer_checkout_chance: CustomerCheckoutChance
    local_checkout_fill: LocalCheckoutFill