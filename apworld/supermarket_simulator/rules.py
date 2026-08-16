from worlds.generic.Rules import set_rule, add_rule
from BaseClasses import CollectionState
from .items import PROGRESSIVE_SECTION_ITEM, PROGRESSIVE_STORAGE_ITEM


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
        # --- Goal: Reach Max Store Level ---
        max_level = self.options.max_store_level.value
        goal_loc_name = f"Store Level {max_level}"
        set_rule(
            victory_location,
            lambda state, target=goal_loc_name: state.can_reach(target, "Location", player)
        )

    elif goal_value == 1:
        # --- Goal: Reach Max Days Completed ---
        max_days = self.options.max_days_completed.value
        goal_loc_name = f"Day {max_days} Completed"
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

