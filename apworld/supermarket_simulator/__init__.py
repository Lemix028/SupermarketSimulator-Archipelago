from worlds.AutoWorld import World
from BaseClasses import Region, Entrance, Location, ItemClassification
from .items import item_table, SupermarketItem, dlc_licenses
from .locations import location_table
from .options import SupermarketOptions
from .webhost import SupermarketSimulatorWebWorld
from .rules import set_rules

class SupermarketLocation(Location):
    game = "Supermarket Simulator"

class SupermarketWorld(World):
    """
    Supermarket Simulator is a retail management game where you build and run your very own
    supermarket from the ground up. Start with an empty store, unlock product licenses to expand
    your shelves, hire staff, purchase furniture, and grow your business day by day.

    In Archipelago, your product licenses, store sections, furniture, vehicles, staff members,
    storage upgrades, and loan authorizations are all randomized and scattered across the multiworld.
    Progress through your store levels and operating days to send checks to other players,
    while waiting for the items you need to keep your shelves stocked and your customers happy.
    """
    game = "Supermarket Simulator"
    topology_present = False
    required_client_version = (0, 6, 7)

    item_name_to_id = {name: data.id for name, data in item_table.items()}
    location_name_to_id = {name: data.id for name, data in location_table.items()}
    options_dataclass = SupermarketOptions

    web = SupermarketSimulatorWebWorld()

    item_name_groups = {
        "Licenses": {name for name in item_table if name.startswith("License ")},
        "Sections": {name for name in item_table if name.startswith("Section ")},
        "Furniture": {
            "Normal Shelf", "Single Shelf", "Half Shelf", "Shelf Inner Corner", "Shelf Outer Corner",
            "Shelf Quad", "Shelf Quad Half", "Fridge Single", "Double Fridge", "Fridge Mini",
            "Display Fridge Single", "Display Fridge Double", "Single Freezer", "Freezer", "Triple Freezer",
            "Checkout Counter", "Checkout Counter Mirrored", "Small Rack", "Tall Rack", "Spot Light",
            "Self Checkout Counter", "Self Checkout Counter Mirrored", "Speaker", "Category Sign",
            "Security Camera", "Security Antenna", "Scale", "Produce Stall", "Trash Can"
        },
        "Vehicles": {
            "Skateboard", "Bicycle", "Scooter", "Sedan", "Pickup Truck"
        },
        "Storage": {f"Storage Room Upgrade {i}" for i in range(1, 21)},
        "Loans": {f"Loan Authorization {i}" for i in range(1, 7)}
    }

    location_name_groups = {
        "Store Levels": {f"Store Level {i}" for i in range(2, 201)},
        "Days Completed": {f"Day {i} Completed" for i in range(1, 1001)},
        "Storage Upgrades": {f"Storage Room Upgrade {i}" for i in range(1, 21)},
        "Section Upgrades": {f"Section Room Upgrade {i}" for i in range(1, 33)},
        "Money Milestones": {f"Earn {i}$" for i in range(1000, 500001, 1000)},
        "License Purchases": {f"Purchase License {i}" for i in range(21, 71)},
        "Customer Checkouts": {f"Customer Checkout {i}" for i in range(1, 101)}
    }

    def create_regions(self) -> None:
        menu_region = Region("Menu", self.player, self.multiworld)
        main_region = Region("Store", self.player, self.multiworld)

        # Connect Menu -> Store (modern API: Region.connect)
        menu_region.connect(main_region, "Enter Store")

        # 1. Store Level Locations
        max_level = self.options.max_store_level.value
        level_interval = self.options.store_level_interval.value
        for level in range(level_interval, max_level + 1, level_interval):
            loc_name = f"Store Level {level}"
            if loc_name in location_table:
                location = SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region)
                main_region.locations.append(location)

        # 2. Days Completed Locations
        max_days = self.options.max_days_completed.value
        days_interval = self.options.days_completed_interval.value
        for day in range(days_interval, max_days + 1, days_interval):
            loc_name = f"Day {day} Completed"
            if loc_name in location_table:
                location = SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region)
                main_region.locations.append(location)

        # 3. Storage Room Upgrade Locations
        if self.options.enable_storage_locks.value:
            for upgrade in range(1, 21):
                loc_name = f"Storage Room Upgrade {upgrade}"
                if loc_name in location_table:
                    location = SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region)
                    main_region.locations.append(location)

        # 4. Money Milestone Locations
        if self.options.enable_money_milestones.value:
            max_money = self.options.max_money_milestone.value
            money_interval = self.options.money_milestone_interval.value
            for money in range(money_interval, max_money + 1, money_interval):
                loc_name = f"Earn {money}$"
                if loc_name in location_table:
                    location = SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region)
                    main_region.locations.append(location)

        # 5. Section Room Locations
        if self.options.enable_section_locations.value:
            for upgrade in range(1, 33):
                loc_name = f"Section Room Upgrade {upgrade}"
                if loc_name in location_table:
                    location = SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region)
                    main_region.locations.append(location)

        # 6. License Purchase Locations (Excludes starting licenses and excluded licenses)
        starting_licenses = self.get_starting_items()
        excluded_licenses = set(self.options.exclude_licenses.value)
        active_dlcs = self.options.active_dlcs.value

        for lic_name in self.item_name_groups["Licenses"]:
            if lic_name in starting_licenses or lic_name in excluded_licenses:
                continue

            is_dlc = False
            for dlc_key, dlc_items in dlc_licenses.items():
                if lic_name in dlc_items:
                    is_dlc = True
                    if dlc_key in active_dlcs:
                        lic_id = dlc_items[lic_name].id - 100
                        loc_name = f"Purchase License {lic_id}"
                        if loc_name in location_table:
                            main_region.locations.append(SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region))
                    break
            if not is_dlc:
                lic_id = item_table[lic_name].id - 100
                loc_name = f"Purchase License {lic_id}"
                if loc_name in location_table:
                    main_region.locations.append(SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region))

        # 7. Customer Checkout Locations
        checkout_loc_count = self.options.customer_checkout_locations.value
        for count in range(1, checkout_loc_count + 1):
            loc_name = f"Customer Checkout {count}"
            if loc_name in location_table:
                main_region.locations.append(SupermarketLocation(self.player, loc_name, location_table[loc_name].id, main_region))

        # Victory event location (address=None marks it as a non-network event)
        victory_location = SupermarketLocation(self.player, "Victory", None, main_region)
        main_region.locations.append(victory_location)

        self.multiworld.regions.append(menu_region)
        self.multiworld.regions.append(main_region)

    def get_starting_items(self) -> set:
        """Collects starting items and guarantees the player can always start the game."""
        starting = set()
        starting.update(self.options.starting_licenses.value)
        starting.update(self.options.starting_vehicles.value)
        starting.update(self.options.starting_furniture.value)
        
        # Guarantee at least one license to prevent softlocking at start
        has_license = any(lic in starting for lic in self.item_name_groups["Licenses"])
        if not has_license:
            starting.add("License 21")
            
                
        return starting

    def get_starting_item_ids(self, option_name: str) -> list:
        """Translates the starting item names from the options into their corresponding IDs."""
        ids = []
        option_list = getattr(self.options, option_name).value
        for name in option_list:
            if name in item_table:
                ids.append(item_table[name].id)
        return ids

    def create_items(self) -> None:
        starting_items = self.get_starting_items()
        total_locations = len(self.multiworld.get_locations(self.player))
        # Subtract 1 for the Victory event location which has no network address
        total_locations -= 1

        # Push all starting items to precollected using the documented API
        for item_name in starting_items:
            self.multiworld.push_precollected(self.create_item(item_name))

        # Gather all other potential progression items
        progression_pool = []
        active_dlcs = self.options.active_dlcs.value

        if "dlc_vending" in active_dlcs:
            vending_slots_count = self.options.vending_machine_slots.value
            for _ in range(vending_slots_count):
                progression_pool.append("Vending Machine Slot")

        for item_name, item_data in item_table.items():
            if item_data.classification in [ItemClassification.filler, ItemClassification.trap]:
                continue

            if item_name in starting_items:
                continue

            if item_name == "Vending Machine Slot":
                continue

            # DLC Handling
            is_dlc_item = False
            dlc_is_active = True
            matching_dlcs = []
            
            for dlc_key, licenses_dict in dlc_licenses.items():
                if item_name in licenses_dict:
                    is_dlc_item = True
                    matching_dlcs.append(dlc_key)
            
            if is_dlc_item:
                dlc_is_active = any(dlc in active_dlcs for dlc in matching_dlcs)
                if not dlc_is_active:
                    continue
            


            if item_name in self.item_name_groups["Vehicles"] and not self.options.enable_vehicle_locks.value:
                self.multiworld.push_precollected(self.create_item(item_name))
                continue
            if item_name in self.item_name_groups["Furniture"] and not self.options.enable_furniture_locks.value:
                self.multiworld.push_precollected(self.create_item(item_name))
                continue
            if item_name in self.item_name_groups["Storage"] and not self.options.enable_storage_locks.value:
                self.multiworld.push_precollected(self.create_item(item_name))
                continue
            if item_name in self.item_name_groups["Loans"] and not self.options.enable_loan_locks.value:
                self.multiworld.push_precollected(self.create_item(item_name))
                continue

            progression_pool.append(item_name)
            
        if len(progression_pool) > total_locations:
            raise Exception(f"Not enough locations ({total_locations}) to fit all mandatory progression items ({len(progression_pool)}). Please increase your level/day caps or intervals.")    
        # Shuffle pool to ensure randomization of item placement
        self.multiworld.random.shuffle(progression_pool)

        itempool = []
        space_left = total_locations

        # Fit as many progression items as we have location slots
        for item_name in progression_pool[:space_left]:
            itempool.append(self.create_item(item_name))
            space_left -= 1

        # Fill remaining slots with boosters and traps
        if space_left > 0:
            disabled_traps = self.options.disabled_traps.value
            all_traps = ["Tax Audit Trap", "Dust Storm Trap", "Power Outage Trap", "Trash Flood Trap", "Expired Products Trap", "Robbery Trap"]
            active_traps = [trap for trap in all_traps if trap not in disabled_traps]

            filler_choices = ["Money Boost", "XP Boost"]
            filler_weights = [self.options.filler_money_weight.value, self.options.filler_xp_weight.value]

            if self.options.enable_blackfriday_events:
                filler_choices.append("Blackfriday")
                filler_weights.append(self.options.filler_blackfriday_weight.value)

            if sum(filler_weights) == 0:
                filler_weights = [50] * len(filler_choices)

            for _ in range(space_left):
                if self.options.enable_traps and active_traps and self.multiworld.random.randint(1, 100) <= self.options.trap_frequency.value:
                    chosen_trap = self.multiworld.random.choice(active_traps)
                    itempool.append(self.create_item(chosen_trap))
                else:
                    chosen_filler = self.multiworld.random.choices(filler_choices, weights=filler_weights, k=1)[0]
                    itempool.append(self.create_item(chosen_filler))

      
        self.multiworld.itempool.extend(itempool)

    def create_item(self, name: str) -> SupermarketItem:
        # Victory is an event item with no network ID
        if name == "Victory":
            return SupermarketItem(name, ItemClassification.progression, None, self.player)
        item_data = item_table[name]

        # Set classification to useful if the item is excluded from the goal, otherwise it would be progression
        classification = item_data.classification
        if name in self.options.exclude_licenses.value:
            classification = ItemClassification.useful

        return SupermarketItem(name, classification, item_data.id, self.player)

    def generate_basic(self) -> None:
        """Place the locked Victory event item at the Victory event location."""
        victory_location = self.multiworld.get_location("Victory", self.player)
        victory_location.place_locked_item(self.create_item("Victory"))

    def set_rules(self) -> None:
        from .rules import set_rules as _set_rules
        _set_rules(self)

    def get_filler_item_name(self) -> str:
        """Returns a random booster/trap item to be used as filler, avoiding progression items."""
        choices = ["Money Boost", "XP Boost"]
        weights = [self.options.filler_money_weight.value, self.options.filler_xp_weight.value]

        if self.options.enable_blackfriday_events:
            choices.append("Blackfriday")
            weights.append(self.options.filler_blackfriday_weight.value)

        # Fallback if player set all weights to 0
        if sum(weights) == 0:
            return self.multiworld.random.choice(choices)

        return self.multiworld.random.choices(choices, weights=weights, k=1)[0]

    def generate_early(self) -> None:
        if self.options.store_level_interval.value > self.options.max_store_level.value:
            raise Exception("Store level interval cannot be larger than the maximum store level.") 

        if self.options.days_completed_interval.value > self.options.max_days_completed.value:
            raise Exception("Days completed interval cannot be larger than the maximum days completed.") 

    def fill_slot_data(self) -> dict:
        # Calculate total active licenses (base + active DLCs)
        from .items import item_table, dlc_licenses
        active_dlcs = self.options.active_dlcs.value
        active_licenses = []

        for name in item_table:
            if not name.startswith("License "):
                continue

            # Determine if this license belongs to any DLC
            parent_dlc = None
            for dlc_key, dlc_items in dlc_licenses.items():
                if name in dlc_items:
                    parent_dlc = dlc_key
                    break

            if parent_dlc is None:
                # Base game license - always active
                active_licenses.append(name)
            elif parent_dlc in active_dlcs:
                # DLC license - active only if the DLC is selected
                active_licenses.append(name)

        active_licenses_for_goal = [name for name in active_licenses if name not in self.options.exclude_licenses.value]

        # Calculate game IDs for all excluded licenses (AP ID - 100)
        excluded_ids = []
        for name in self.options.exclude_licenses.value:
            if name in item_table:
                excluded_ids.append(item_table[name].id - 100)

        # Calculate game IDs for all required licenses (AP ID - 100)
        required_license_ids = []
        for name in active_licenses_for_goal:
            if name in item_table:
                required_license_ids.append(item_table[name].id - 100)

        return {
            "goal": int(self.options.goal.value),
            "max_store_level": self.options.max_store_level.value,
            "store_level_interval": self.options.store_level_interval.value,
            "max_days_completed": self.options.max_days_completed.value,
            "days_completed_interval": self.options.days_completed_interval.value,
            "enable_furniture_locks": int(self.options.enable_furniture_locks.value),
            "enable_vehicle_locks": int(self.options.enable_vehicle_locks.value),
            "enable_storage_locks": int(self.options.enable_storage_locks.value),
            "enable_section_locations": int(self.options.enable_section_locations.value),
            "enable_loan_locks": int(self.options.enable_loan_locks.value),       
            "default_licenses": self.get_starting_item_ids("starting_licenses"),
            "default_vehicles": self.get_starting_item_ids("starting_vehicles"),
            "default_furniture": self.get_starting_item_ids("starting_furniture"),
            "enable_money_milestones": int(self.options.enable_money_milestones.value),
            "max_money_milestone": self.options.max_money_milestone.value,
            "money_milestone_interval": self.options.money_milestone_interval.value,
            "vending_machine_slots": self.options.vending_machine_slots.value,
            "price_randomization": int(self.options.price_randomization.value),
            "total_licenses": len(active_licenses_for_goal),
            "excluded_licenses": excluded_ids,
            "required_licenses": required_license_ids,
            "checkout_income_multiplier": self.options.checkout_income_multiplier.value,
            "starting_cash": self.options.starting_cash.value,
            "free_customizables": int(self.options.free_customizables.value),
            "customer_checkout_locations": self.options.customer_checkout_locations.value,
            "customer_checkout_chance": self.options.customer_checkout_chance.value,
            "seed": self.multiworld.seed_name,
        }
