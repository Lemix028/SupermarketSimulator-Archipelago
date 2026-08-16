import unittest
from test.bases import WorldTestBase
from BaseClasses import ItemClassification
from worlds.supermarket_simulator.options import StartingLicenses, DisabledTraps, StartingFurniture
from worlds.supermarket_simulator.items import PROGRESSIVE_SECTION_ITEM, PROGRESSIVE_STORAGE_ITEM
from Fill import distribute_items_restrictive

class SupermarketSimulatorTestBase(WorldTestBase):
    game = "Supermarket Simulator"

    def can_reach(self, location_name: str) -> bool:
        """Helper to safely check if a location is reachable with the current world state."""
        try:
            location = self.multiworld.get_location(location_name, self.player)
            return location.can_reach(self.multiworld.state)
        except KeyError:
            return False



class TestSupermarketSimulatorDefault(SupermarketSimulatorTestBase):
    pass


class TestSupermarketSimulatorGoalAllLicenses(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,  # All Licenses
        "max_store_level": 50,
        "max_days_completed": 100,
    }


class TestSupermarketSimulatorNoLocks(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "max_days_completed": 100,
        "enable_furniture_locks": False,
        "enable_vehicle_locks": False,
        "enable_storage_locks": False,
        "enable_loan_locks": False,
        "enable_money_milestones": False,
    }


class TestSupermarketSimulatorExtremeOptions(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 5,
        "store_level_interval": 5,
        "max_days_completed": 5,
        "days_completed_interval": 5,
        "enable_furniture_locks": False,
        "enable_storage_locks": False,
        "enable_section_locations": False,
        "enable_loan_locks": False,
        "enable_money_milestones": True, 
        "max_money_milestone": 100000, 
        "money_milestone_interval": 1000
    }


class TestSupermarketSimulatorAllDLCsActive(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "max_days_completed": 100,
        "active_dlcs": {
            "dlc_ice_cream", "dlc_bakery", "dlc_vending", "dlc_essentials",
            "dlc_hardware", "dlc_electronics", "dlc_clothing"
        }
    }

    def test_dlc_items_present_in_pool(self) -> None:
        """Ensures that DLC-specific licenses are present in the pool when the DLCs are activated."""
        self.world_setup()
        pool_item_names = [item.name for item in self.multiworld.itempool if item.player == self.player]
        self.assertIn("License 69", pool_item_names)
        self.assertIn("License 66", pool_item_names)
        self.assertIn("License 51", pool_item_names)


class TestSupermarketSimulatorNoDLCsActive(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "max_days_completed": 100,
        "active_dlcs": set() 
    }

    def test_dlc_items_absent_from_pool(self) -> None:
        """Ensure that no DLC items are present in the item pool when no DLCs are selected."""
        self.world_setup()
        pool_item_names = [item.name for item in self.multiworld.itempool if item.player == self.player]
        self.assertIn("License 22", pool_item_names)
        self.assertNotIn("License 69", pool_item_names)
        self.assertNotIn("License 51", pool_item_names)


# --- SPECIFIC ITEM & LOGIC TESTS ---

class TestSupermarketSimulatorMinimalStartingItems(SupermarketSimulatorTestBase):
    options = {
        "enable_furniture_locks": True,
        "starting_licenses": [], 
        "starting_furniture": [],  
    }

    def test_safety_net_only_injects_license(self) -> None:
        """Test minimal starting items with furniture locks enabled."""
        self.world_setup()
        precollected_names = [item.name for item in self.multiworld.precollected_items[self.player]]
        self.assertIn("License 21", precollected_names)
        self.assertNotIn("Normal Shelf", precollected_names)


class TestSupermarketSimulatorFurnitureLocked(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "max_days_completed": 100,
        "enable_furniture_locks": True,
    }

    def test_furniture_in_pool(self) -> None:
        self.world_setup()
        pool_item_names = [item.name for item in self.multiworld.itempool if item.player == self.player]
        self.assertIn("Double Fridge", pool_item_names)


class TestSupermarketSimulatorFurnitureUnlocked(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "max_days_completed": 100,
        "enable_furniture_locks": False,
    }

    def test_furniture_precollected(self) -> None:
        self.world_setup()
        pool_item_names = [item.name for item in self.multiworld.itempool if item.player == self.player]
        self.assertNotIn("Normal Shelf", pool_item_names)
        precollected_names = [item.name for item in self.multiworld.precollected_items[self.player]]
        self.assertIn("Normal Shelf", precollected_names)


class TestSupermarketSimulatorLogic(SupermarketSimulatorTestBase):
    def test_start_feasibility(self) -> None:
        self.world_setup()
        for item in self.multiworld.precollected_items[self.player]:
            self.multiworld.state.collect(item)
        self.assertTrue(self.can_reach("Store Level 5"))


class TestSupermarketSimulatorMoneyMilestones(SupermarketSimulatorTestBase):
    options = {
        "enable_money_milestones": True,
        "max_money_milestone": 20000,
        "money_milestone_interval": 5000,
    }

    def test_money_milestone_locations_count(self) -> None:
        self.world_setup()
        money_locations = [loc for loc in self.multiworld.get_locations(self.player) if loc.name.startswith("Earn ")]
        self.assertEqual(len(money_locations), 4)



class TestSupermarketSimulatorOptionsValidation(unittest.TestCase):
    def test_invalid_license_raises_error(self) -> None:
        options = StartingLicenses(["License 10654"])
        with self.assertRaises(ValueError):
            options.verify(None, "Player1", {})

    def test_invalid_trap_raises_error(self) -> None:
        options = DisabledTraps(["Invalid Trap"])
        with self.assertRaises(ValueError):
            options.verify(None, "Player1", {})

    def test_invalid_furniture_raises_error(self) -> None:
        options = StartingFurniture(["Checkout Counter Deluxe"])
        with self.assertRaises(ValueError):
            options.verify(None, "Player1", {})


# --- ADDITIONAL INTEGRITY AND VICTORY LOGIC TESTS ---

class TestSupermarketSimulatorLevelGoalVictory(SupermarketSimulatorTestBase):
    options = {
        "goal": 0,  # Level Goal
        "max_store_level": 100,
        "store_level_interval": 5,
        "max_days_completed": 100,
    }

    def test_level_goal_victory(self) -> None:
        self.world_setup()
        victory_loc = self.multiworld.get_location("Victory", self.player)
        
        # Level milestones are logically free (no item locks), so they and victory are reachable at start
        self.assertTrue(self.can_reach("Store Level 100"))
        self.assertTrue(victory_loc.can_reach(self.multiworld.state))
        self.assertFalse(self.multiworld.completion_condition[self.player](self.multiworld.state))
        
        # Collect Victory event item and verify completion
        self.multiworld.state.collect(victory_loc.item)
        self.assertTrue(self.multiworld.completion_condition[self.player](self.multiworld.state))


class TestSupermarketSimulatorDaysGoalVictory(SupermarketSimulatorTestBase):
    options = {
        "goal": 1,  # Days Goal
        "max_store_level": 100,
        "max_days_completed": 100,
        "days_completed_interval": 5,
    }

    def test_days_goal_victory(self) -> None:
        self.world_setup()
        victory_loc = self.multiworld.get_location("Victory", self.player)
        
        # Completed days milestones are logically free (no item locks), so they and victory are reachable at start
        self.assertTrue(self.can_reach("Day 100 Completed"))
        self.assertTrue(victory_loc.can_reach(self.multiworld.state))
        self.assertFalse(self.multiworld.completion_condition[self.player](self.multiworld.state))
        
        # Collect Victory event item and verify completion
        self.multiworld.state.collect(victory_loc.item)
        self.assertTrue(self.multiworld.completion_condition[self.player](self.multiworld.state))


class TestSupermarketSimulatorAllLicensesGoalVictory(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,  # All Licenses Goal
        "active_dlcs": {"dlc_bakery"},  # Base + Bakery DLC active
    }

    def test_all_licenses_goal_victory(self) -> None:
        self.world_setup()
        victory_loc = self.multiworld.get_location("Victory", self.player)
        
        # Collect everything except License 22
        for item in self.multiworld.get_items():
            if item.player == self.player and item.name != "License 22":
                self.multiworld.state.collect(item)
                
        # Victory should not be reachable because License 22 is missing
        self.assertFalse(victory_loc.can_reach(self.multiworld.state))
        
        # Find and collect License 22 to check victory is now achieved
        for item in self.multiworld.itempool:
            if item.player == self.player and item.name == "License 22":
                self.multiworld.state.collect(item)
                break
                
        self.assertTrue(victory_loc.can_reach(self.multiworld.state))


class TestSupermarketSimulatorSafetyNetNotTriggered(SupermarketSimulatorTestBase):
    options = {
        "starting_licenses": {"License 22"},
    }

    def test_safety_net_not_triggered(self) -> None:
        self.world_setup()
        precollected_names = [item.name for item in self.multiworld.precollected_items[self.player]]
        self.assertIn("License 22", precollected_names)
        # License 21 is the default game starting license and is always included
        self.assertIn("License 21", precollected_names)


class TestSupermarketSimulatorDynamicSlotData(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,
        "active_dlcs": {"dlc_ice_cream", "dlc_bakery"},
    }

    def test_slot_data_total_licenses(self) -> None:
        self.world_setup()
        slot_data = self.multiworld.worlds[self.player].fill_slot_data()
        
        # Expected licenses: 30 base (License 21-50) + 1 ice cream (License 69) + 3 bakery (License 66-68) = 34
        self.assertEqual(slot_data["total_licenses"], 34)


class TestSupermarketSimulatorExcludedLicensesVictory(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,  # All Licenses Goal
        "active_dlcs": {"dlc_bakery"},
        "exclude_licenses": {"License 22", "License 66"},
    }

    def test_excluded_licenses_not_required(self) -> None:
        self.world_setup()
        victory_loc = self.multiworld.get_location("Victory", self.player)
        
        # Collect everything except License 22 and License 66 (which are excluded)
        for item in self.multiworld.get_items():
            if item.player == self.player and item.name not in ["License 22", "License 66"]:
                self.multiworld.state.collect(item)
                
        # Victory should be reachable even without License 22 and 66!
        self.assertTrue(victory_loc.can_reach(self.multiworld.state))


class TestSupermarketSimulatorExcludedLicensesSlotData(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,
        "active_dlcs": {"dlc_ice_cream", "dlc_bakery"},
        "exclude_licenses": {"License 22", "License 69"}, # 1 base, 1 ice cream dlc license
    }

    def test_slot_data_excludes_licenses(self) -> None:
        self.world_setup()
        slot_data = self.multiworld.worlds[self.player].fill_slot_data()
        
        # Expected licenses: 30 base + 1 ice cream + 3 bakery = 34
        # Excluded: 2 licenses
        # Total licenses should be 32
        self.assertEqual(slot_data["total_licenses"], 32)
        
        # Excluded licenses list should have the game IDs for License 22 and 69
        # License 22 AP ID is 122 -> Game ID is 22
        # License 69 AP ID is 169 -> Game ID is 69
        self.assertIn(22, slot_data["excluded_licenses"])
        self.assertIn(69, slot_data["excluded_licenses"])
        
        # required_licenses should have length 32
        self.assertEqual(len(slot_data["required_licenses"]), 32)
        # Excluded licenses should NOT be in required_licenses
        self.assertNotIn(22, slot_data["required_licenses"])
        self.assertNotIn(69, slot_data["required_licenses"])
        # Some active non-excluded licenses should be in required_licenses
        self.assertIn(21, slot_data["required_licenses"]) # base
        self.assertIn(23, slot_data["required_licenses"]) # base
        self.assertIn(66, slot_data["required_licenses"]) # bakery dlc license


class TestSupermarketSimulatorExcludedLicensesClassification(SupermarketSimulatorTestBase):
    options = {
        "goal": 2,
        "active_dlcs": {"dlc_bakery"},
        "exclude_licenses": {"License 22", "License 66"},
    }

    def test_excluded_licenses_are_useful(self) -> None:
        self.world_setup()
        
        # Test item creation classification
        item_22 = self.multiworld.worlds[self.player].create_item("License 22")
        item_66 = self.multiworld.worlds[self.player].create_item("License 66")
        item_23 = self.multiworld.worlds[self.player].create_item("License 23") # not excluded
        
        self.assertEqual(item_22.classification, ItemClassification.useful)
        self.assertEqual(item_66.classification, ItemClassification.useful)
        self.assertEqual(item_23.classification, ItemClassification.progression)


class TestSupermarketSimulatorLocationRules(SupermarketSimulatorTestBase):
    options = {
        "max_store_level": 50,
        "store_level_interval": 1,
        "max_days_completed": 100,
        "days_completed_interval": 1,
        "enable_storage_locks": True,
        "enable_section_locations": True,
        "enable_money_milestones": True,
        "max_money_milestone": 5000,
        "money_milestone_interval": 1000,
        "customer_checkout_locations": 10,
    }

    def test_milestone_chaining_logic(self) -> None:
        self.world_setup()

        # Store Level 1 location should NOT exist (starts at Store Level 2)
        with self.assertRaises(KeyError):
            self.multiworld.get_location("Store Level 1", self.player)

        # Store Level 2 should be reachable initially
        self.assertTrue(self.can_reach("Store Level 2"))
        # Store Level 3 requires Store Level 2
        level_3_loc = self.multiworld.get_location("Store Level 3", self.player)
        self.assertTrue(level_3_loc.can_reach(self.multiworld.state))

        # Day 1 Completed should be reachable
        self.assertTrue(self.can_reach("Day 1 Completed"))

        # Earn 1000$ should be reachable initially
        self.assertTrue(self.can_reach("Earn 1000$"))
        # Earn 2000$ should be reachable when Earn 1000$ is reached
        earn_2000_loc = self.multiworld.get_location("Earn 2000$", self.player)
        self.assertTrue(earn_2000_loc.can_reach(self.multiworld.state))

        # Storage Room Upgrade 1 requires one received storage upgrade item
        storage_1_loc = self.multiworld.get_location("Storage Room Upgrade 1", self.player)
        self.assertFalse(storage_1_loc.can_reach(self.multiworld.state))
        self.collect(self.get_items_by_name(PROGRESSIVE_STORAGE_ITEM)[0])
        self.assertTrue(storage_1_loc.can_reach(self.multiworld.state))

        # Starting license (License 21) should NOT generate a Purchase License 21 location
        with self.assertRaises(KeyError):
            self.multiworld.get_location("Purchase License 21", self.player)

        # Purchase License 22 location should exist and require License 22 item
        lic_22_loc = self.multiworld.get_location("Purchase License 22", self.player)
        self.assertFalse(lic_22_loc.can_reach(self.multiworld.state))
        self.collect_by_name("License 22")
        self.assertTrue(lic_22_loc.can_reach(self.multiworld.state))

        # Customer Checkout 1 location should exist and be reachable initially
        self.assertTrue(self.can_reach("Customer Checkout 1"))
        checkout_2_loc = self.multiworld.get_location("Customer Checkout 2", self.player)
        self.assertTrue(checkout_2_loc.can_reach(self.multiworld.state))

    def test_room_upgrades_follow_received_item_counts(self) -> None:
        self.world_setup()

        storage_1 = self.multiworld.get_location("Storage Room Upgrade 1", self.player)
        storage_2 = self.multiworld.get_location("Storage Room Upgrade 2", self.player)
        storage_items = self.get_items_by_name(PROGRESSIVE_STORAGE_ITEM)
        self.assertEqual(len(storage_items), 20)
        self.collect(storage_items[0])
        self.assertTrue(storage_1.can_reach(self.multiworld.state))
        self.assertFalse(storage_2.can_reach(self.multiworld.state))
        self.collect(storage_items[1])
        self.assertTrue(storage_2.can_reach(self.multiworld.state))

        section_1 = self.multiworld.get_location("Section Room Upgrade 1", self.player)
        section_2 = self.multiworld.get_location("Section Room Upgrade 2", self.player)
        section_32 = self.multiworld.get_location("Section Room Upgrade 32", self.player)
        section_items = self.get_items_by_name(PROGRESSIVE_SECTION_ITEM)
        self.assertEqual(len(section_items), 32)
        self.collect(section_items[0])
        self.assertTrue(section_1.can_reach(self.multiworld.state))
        self.assertFalse(section_2.can_reach(self.multiworld.state))
        self.assertFalse(section_32.can_reach(self.multiworld.state))
        self.collect(section_items[1])
        self.assertTrue(section_2.can_reach(self.multiworld.state))

        self.collect(section_items[2:31])
        self.assertFalse(section_32.can_reach(self.multiworld.state))
        self.collect(section_items[31])
        self.assertTrue(section_32.can_reach(self.multiworld.state))


class TestSupermarketSimulatorCustomerCheckoutFill(SupermarketSimulatorTestBase):
    options = {
        "customer_checkout_locations": 200,
        "local_checkout_fill": 90,
    }

    def test_checkout_fill_prefills_locations(self) -> None:
        self.world_setup()
        checkout_locations = [loc for loc in self.multiworld.get_locations(self.player) if loc.name.startswith("Customer Checkout ")]
        self.assertEqual(len(checkout_locations), 200)

        # 90% of 200 = 180 locations pre-filled locally, 20 remaining for global pool
        prefilled = [loc for loc in checkout_locations if loc.item is not None]
        unfilled = [loc for loc in checkout_locations if loc.item is None]
        self.assertEqual(len(prefilled), 180)
        self.assertEqual(len(unfilled), 20)


class TestSupermarketSimulatorDefaultLocalFill(SupermarketSimulatorTestBase):
    options = {
        "customer_checkout_locations": 200,
    }

    def test_default_checkout_fill(self) -> None:
        self.world_setup()
        checkout_locations = [loc for loc in self.multiworld.get_locations(self.player) if loc.name.startswith("Customer Checkout ")]
        self.assertEqual(len(checkout_locations), 200)

        # Default 60% of 200 = 120 locations pre-filled locally, 80 remaining for global pool
        prefilled = [loc for loc in checkout_locations if loc.item is not None]
        unfilled = [loc for loc in checkout_locations if loc.item is None]
        self.assertEqual(len(prefilled), 120)
        self.assertEqual(len(unfilled), 80)


class TestSupermarketSimulatorZeroCheckouts(SupermarketSimulatorTestBase):
    options = {
        "customer_checkout_locations": 0,
    }

    def test_zero_checkouts_generation(self) -> None:
        self.world_setup()
        checkout_locations = [loc for loc in self.multiworld.get_locations(self.player) if loc.name.startswith("Customer Checkout ")]
        self.assertEqual(len(checkout_locations), 0)


class TestSupermarketSimulatorFullLocalFill(SupermarketSimulatorTestBase):
    options = {
        "customer_checkout_locations": 100,
        "local_checkout_fill": 100,
    }

    def test_full_local_fill(self) -> None:
        self.world_setup()
        checkout_locations = [loc for loc in self.multiworld.get_locations(self.player) if loc.name.startswith("Customer Checkout ")]
        self.assertEqual(len(checkout_locations), 100)
        prefilled = [loc for loc in checkout_locations if loc.item is not None]
        unfilled = [loc for loc in checkout_locations if loc.item is None]
        self.assertEqual(len(prefilled), 100)
        self.assertEqual(len(unfilled), 0)

