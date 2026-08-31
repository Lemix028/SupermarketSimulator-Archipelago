from worlds.generic.Rules import set_rule, add_rule, add_item_rule
from BaseClasses import CollectionState, ItemClassification
from .items import (
    DAY_AND_LEVEL_PROGRESSION_ITEMS,
    PROGRESSIVE_SECTION_ITEM,
    PROGRESSIVE_STORAGE_ITEM,
)


def _numeric_requirement(location_name: str) -> int:
    """Return the numeric requirement from either supported milestone name."""
    if location_name.startswith("Store Level "):
        return int(location_name.removeprefix("Store Level "))
    return int(location_name.removeprefix("Day ").removesuffix(" Completed"))


def _add_percentage_tier_rules(world, locations) -> None:
    """Split one sorted milestone family into five percentage-based tiers."""
    locations = sorted(locations, key=lambda location: _numeric_requirement(location.name))
    location_count = len(locations)
    if not location_count:
        return

    player = world.player
    total_items = world.total_relevant_progression_items
    for index, location in enumerate(locations):
        tier = min(4, index * 5 // location_count)
        required_items = (total_items * tier + 4) // 5 if total_items else 0
        add_rule(
            location,
            lambda state, required=required_items: sum(
                state.count(item_name, player)
                for item_name in DAY_AND_LEVEL_PROGRESSION_ITEMS
            ) >= required,
        )

        if tier == 4 and world.options.exclude_progression_from_late_checks.value:
            add_item_rule(
                location,
                lambda item: not (item.classification & ItemClassification.progression),
            )


def set_rules(self) -> None:
    """
    Sets location access rules and Victory completion condition.

    Item Requirements:
      - Storage Room Upgrade N requires N received storage upgrade items (if locked)
      - Section Room Upgrade N requires N received section items
      - Purchase License X requires License X item

    Goals:
      0 (Level)       - Reach the highest generated store level milestone.
      1 (Days)        - Reach the highest generated completed-days milestone.
      2 (All Licenses)- Collect every product license available in the item pool.
    """
    player = self.player

    day_locations = []
    store_level_locations = []
    for location in self.multiworld.get_locations(player):
        if location.address is None:
            continue
        if location.name.startswith("Day ") and location.name.endswith(" Completed"):
            day_locations.append(location)
        elif location.name.startswith("Store Level "):
            store_level_locations.append(location)

    # Keep the two families independent: changing one interval or maximum must
    # never move checks in the other family between tiers.
    _add_percentage_tier_rules(self, day_locations)
    _add_percentage_tier_rules(self, store_level_locations)

    # 1. Storage Room Upgrade Item Rules
    if self.options.enable_storage_locks.value:
        storage_locs = [
            loc for loc in self.multiworld.get_locations(player)
            if loc.name.startswith("Storage Room Upgrade ") and loc.address is not None
        ]
        for loc in storage_locs:
            upgrade_num = int(loc.name.split()[-1])
            set_rule(
                loc,
                lambda state, required=upgrade_num: state.has(
                    PROGRESSIVE_STORAGE_ITEM, player, required
                ),
            )

    # 2. Section Room Upgrade Item Rules
    if self.options.enable_section_locations.value:
        section_locs = [
            loc for loc in self.multiworld.get_locations(player)
            if loc.name.startswith("Section Room Upgrade ") and loc.address is not None
        ]
        for loc in section_locs:
            upgrade_num = int(loc.name.split()[-1])
            set_rule(
                loc,
                lambda state, required=upgrade_num: state.has(
                    PROGRESSIVE_SECTION_ITEM, player, required
                ),
            )

    # 3. License Purchase Location Rules (Requires having the License item)
    purchase_license_locs = [
        loc for loc in self.multiworld.get_locations(player)
        if loc.name.startswith("Purchase License ") and loc.address is not None
    ]
    for loc in purchase_license_locs:
        lic_id = loc.name.replace("Purchase License ", "")
        item_name = f"License {lic_id}"
        set_rule(loc, lambda state, item=item_name: state.has(item, player))

    # === VICTORY GOAL RULES ===
    goal_value = self.options.goal.value
    victory_location = self.multiworld.get_location("Victory", player)

    if goal_value == 0:
        # --- Goal: Reach Highest Generated Store Level ---
        goal_loc_name = max(
            store_level_locations,
            key=lambda location: _numeric_requirement(location.name),
        ).name
        set_rule(
            victory_location,
            lambda state, target=goal_loc_name: state.can_reach(target, "Location", player)
        )

    elif goal_value == 1:
        # --- Goal: Reach Highest Generated Days Milestone ---
        goal_loc_name = max(
            day_locations,
            key=lambda location: _numeric_requirement(location.name),
        ).name
        set_rule(
            victory_location,
            lambda state, target=goal_loc_name: state.can_reach(target, "Location", player)
        )

    elif goal_value == 2:
        # --- Goal: Collect All Licenses ---
        from .items import item_table, dlc_licenses

        active_dlcs = self.options.active_dlcs.value
        active_license_names = []

        for name in item_table:
            if not name.startswith("License "):
                continue

            parent_dlc = None
            for dlc_key, dlc_items in dlc_licenses.items():
                if name in dlc_items:
                    parent_dlc = dlc_key
                    break

            if parent_dlc is None:
                active_license_names.append(name)
            elif parent_dlc in active_dlcs:
                active_license_names.append(name)

        active_license_names_for_goal = [name for name in active_license_names if name not in self.options.exclude_licenses.value]

        set_rule(
            victory_location,
            lambda state, licenses=active_license_names_for_goal: all(
                state.has(lic, player) for lic in licenses
            )
        )

    self.multiworld.completion_condition[player] = (
        lambda state: state.has("Victory", player)
    )

