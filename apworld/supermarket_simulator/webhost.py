from worlds.AutoWorld import WebWorld
from BaseClasses import Tutorial

item_descriptions = {
    "License 21": "Allows buying and selling standard grocery products.",
    "Money Boost": "Instantly adds a cash bonus to your store's bank account.",
    "XP Boost": "Grants bonus store experience to help reach the next level faster.",
    "Blackfriday": "Triggers a Black Friday event, flooding your store with customers.",
    "Tax Audit Trap": "A surprise tax audit deducts money from your bank account.",
    "Dust Storm Trap": "A dust storm covers your store floors, requiring extensive cleaning.",
    "Power Outage Trap": "A power failure dims your store lights for a period of time.",
    "Trash Flood Trap": "A wave of garbage and dirt floods your store floor.",
    "Expired Products Trap": "Products on a random shelf expire and are removed instantly.",
    "Robbery Trap": "A group of shoplifters spawns and attempts to steal from your store.",
    "Checkout Counter": "Required to process customer purchases. Essential for running your store.",
    "Skateboard": "A basic personal vehicle for faster restocking runs.",
    "Pickup Truck": "The largest vehicle, allowing bulk restocking in a single trip.",
    "Vending Machine Slot": "Unlocks an additional vending machine slot in the computer shop.",
}

location_descriptions = {
    "Store Levels": "Checks triggered by reaching configured store level milestones (starting at Level 2).",
    "Days Completed": "Checks triggered by completing the configured number of operating days.",
    "Storage Upgrades": "Checks triggered by purchasing storage room upgrades (if enabled).",
    "Section Upgrades": "Checks triggered by purchasing section room expansions (if enabled).",
    "Money Milestones": "Checks triggered by earning specific cumulative amounts of money (if enabled).",
    "License Purchases": "Checks triggered by purchasing received product licenses at the store computer.",
    "Customer Checkouts": "Checks triggered probabilistically when customers are checked out at registers.",
}


class SupermarketSimulatorWebWorld(WebWorld):
    theme = "grass"

    bug_report_page = "https://github.com/Lemix028/SupermarketSimulator-Archipelago/issues"

    item_descriptions = item_descriptions
    location_descriptions = location_descriptions

    tutorials = [
        Tutorial(
            tutorial_name="Setup Guide",
            description="A guide to setting up the Archipelago Supermarket Simulator integration.",
            language="English",
            file_name="setup_en.md",
            link="setup/en",
            authors=["Lemix028"]
        )
    ]

    options_presets = {
        "Easy": {
            "goal": "level",
            "max_store_level": 25,
            "store_level_interval": 1,
            "max_days_completed": 50,
            "days_completed_interval": 1,
            "enable_furniture_locks": False,
            "enable_vehicle_locks": False,
            "enable_storage_locks": False,
            "enable_section_locations": True,
            "enable_loan_locks": False,
            "enable_traps": True,
            "trap_frequency": 10,
            "enable_money_milestones": False,
            "price_randomization": "disabled",
        },
        "Normal": {
            "goal": "level",
            "max_store_level": 50,
            "store_level_interval": 1,
            "max_days_completed": 100,
            "days_completed_interval": 1,
            "enable_furniture_locks": True,
            "enable_vehicle_locks": True,
            "enable_storage_locks": True,
            "enable_section_locations": True,
            "enable_loan_locks": True,
            "enable_traps": True,
            "trap_frequency": 20,
            "enable_money_milestones": True,
            "max_money_milestone": 25000,
            "money_milestone_interval": 5000,
            "price_randomization": "balanced",
        },
        "Hard": {
            "goal": "all_licenses",
            "max_store_level": 100,
            "store_level_interval": 2,
            "max_days_completed": 200,
            "days_completed_interval": 2,
            "enable_furniture_locks": True,
            "enable_vehicle_locks": True,
            "enable_storage_locks": True,
            "enable_section_locations": True,
            "enable_loan_locks": True,
            "enable_traps": True,
            "trap_frequency": 40,
            "enable_money_milestones": True,
            "max_money_milestone": 50000,
            "money_milestone_interval": 5000,
            "price_randomization": "chaotic",
        },
    }
