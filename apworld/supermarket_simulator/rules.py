from worlds.generic.Rules import set_rule
from BaseClasses import CollectionState


def set_rules(self) -> None:
    """
    Sets the Victory location rule and the completion condition based on the chosen goal.

    Goals:
      0 (Level)       - Reach the highest generated store level milestone.
      1 (Days)        - Reach the highest generated completed-days milestone.
      2 (All Licenses)- Collect every product license available in the item pool
                        (including active DLC licenses).

    The Victory event location is always placed in the Store region.
    The completion condition requires the player to have the Victory item,
    which is locked at the Victory location.
    """
    goal_value = self.options.goal.value
    victory_location = self.multiworld.get_location("Victory", self.player)

    if goal_value == 0:
        # --- Goal: Reach Max Store Level ---
        # Find the highest level milestone location that was actually generated.
        store_level_locs = [
            loc for loc in self.multiworld.get_locations(self.player)
            if loc.name.startswith("Store Level ") and loc.address is not None
        ]
        if store_level_locs:
            goal_loc = max(store_level_locs, key=lambda loc: int(loc.name.split()[-1]))
            set_rule(
                victory_location,
                lambda state, target=goal_loc: state.can_reach(target.name, "Location", self.player)
            )
        # Fallback: no level locations generated – victory is always reachable
        # (edge-case guard; generate_early() should already prevent this)

    elif goal_value == 1:
        # --- Goal: Reach Max Days Completed ---
        day_locs = [
            loc for loc in self.multiworld.get_locations(self.player)
            if loc.name.startswith("Day ") and loc.name.endswith(" Completed") and loc.address is not None
        ]
        if day_locs:
            goal_loc = max(day_locs, key=lambda loc: int(loc.name.split()[1]))
            set_rule(
                victory_location,
                lambda state, target=goal_loc: state.can_reach(target.name, "Location", self.player)
            )

    elif goal_value == 2:
        # --- Goal: Collect All Licenses ---
        # Collect every license name that is active in this seed (base + active DLCs).
        from .items import item_table, dlc_licenses

        active_dlcs = self.options.active_dlcs.value
        active_license_names = []

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
                active_license_names.append(name)
            elif parent_dlc in active_dlcs:
                # DLC license - active only if the DLC is selected
                active_license_names.append(name)

        active_license_names_for_goal = [name for name in active_license_names if name not in self.options.exclude_licenses.value]

        set_rule(
            victory_location,
            lambda state, licenses=active_license_names_for_goal: all(
                state.has(lic, self.player) for lic in licenses
            )
        )

    # The completion condition is the same for all goals:
    # the player must have received the Victory event item.
    self.multiworld.completion_condition[self.player] = (
        lambda state: state.has("Victory", self.player)
    )
