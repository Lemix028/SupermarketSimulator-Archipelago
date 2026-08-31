from BaseClasses import Item, ItemClassification
from typing import NamedTuple

class ItemData(NamedTuple):
    id: int
    classification: ItemClassification

class SupermarketItem(Item):
    game: str = "Supermarket Simulator"

PROGRESSIVE_SECTION_ITEM = "Progressive Section Room Upgrade"
PROGRESSIVE_STORAGE_ITEM = "Progressive Storage Room Upgrade"
SECTION_UPGRADE_COUNT = 32
STORAGE_UPGRADE_COUNT = 20
PROGRESSIVE_STAFF_COUNTS = {
    "Progressive Cashier": 6,
    "Progressive Janitor": 3,
    "Progressive Restocker": 6,
    "Progressive Security Guard": 2,
    "Progressive Customer Helper": 6,
}
PROGRESSIVE_DLC_STAFF_COUNTS = {
    "dlc_bakery": {"Progressive Baker": 2},
    "dlc_ice_cream": {"Progressive Ice Cream Helper": 2},
}

item_table = {
    # === LICENSES (Offset 100+) ===
    "License 21": ItemData(121, ItemClassification.progression),
    "License 22": ItemData(122, ItemClassification.progression),
    "License 23": ItemData(123, ItemClassification.progression),
    "License 24": ItemData(124, ItemClassification.progression),
    "License 25": ItemData(125, ItemClassification.progression),
    "License 26": ItemData(126, ItemClassification.progression),
    "License 27": ItemData(127, ItemClassification.progression),
    "License 28": ItemData(128, ItemClassification.progression),
    "License 29": ItemData(129, ItemClassification.progression),
    "License 30": ItemData(130, ItemClassification.progression),
    "License 31": ItemData(131, ItemClassification.progression),
    "License 32": ItemData(132, ItemClassification.progression),
    "License 33": ItemData(133, ItemClassification.progression),
    "License 34": ItemData(134, ItemClassification.progression),
    "License 35": ItemData(135, ItemClassification.progression),
    "License 36": ItemData(136, ItemClassification.progression),
    "License 37": ItemData(137, ItemClassification.progression),
    "License 38": ItemData(138, ItemClassification.progression),
    "License 39": ItemData(139, ItemClassification.progression),
    "License 40": ItemData(140, ItemClassification.progression),
    "License 41": ItemData(141, ItemClassification.progression),
    "License 42": ItemData(142, ItemClassification.progression),
    "License 43": ItemData(143, ItemClassification.progression),
    "License 44": ItemData(144, ItemClassification.progression),
    "License 45": ItemData(145, ItemClassification.progression),
    "License 46": ItemData(146, ItemClassification.progression),
    "License 47": ItemData(147, ItemClassification.progression),
    "License 48": ItemData(148, ItemClassification.progression),
    "License 49": ItemData(149, ItemClassification.progression),
    "License 50": ItemData(150, ItemClassification.progression),

    
    # === SECTIONS (Offset 200+) ===
    PROGRESSIVE_SECTION_ITEM: ItemData(201, ItemClassification.progression),

    # === STAFF (Offset 300+) ===
    "Progressive Cashier": ItemData(301, ItemClassification.progression),
    "Progressive Janitor": ItemData(311, ItemClassification.progression),
    "Progressive Restocker": ItemData(321, ItemClassification.progression),
    "Progressive Security Guard": ItemData(331, ItemClassification.progression),
    "Progressive Customer Helper": ItemData(341, ItemClassification.progression),


    # === VEHICLES (Offset 350+) ===
    "Skateboard": ItemData(350, ItemClassification.useful),
    "Bicycle": ItemData(351, ItemClassification.useful),
    "Scooter": ItemData(352, ItemClassification.useful),
    "Sedan": ItemData(353, ItemClassification.useful),
    "Pickup Truck": ItemData(354, ItemClassification.useful),

    # === FURNITURES (Offset 400+) ===
    "Normal Shelf": ItemData(401, ItemClassification.useful),
    "Single Shelf": ItemData(402, ItemClassification.useful),
    "Half Shelf": ItemData(403, ItemClassification.progression),
    "Shelf Inner Corner": ItemData(404, ItemClassification.useful),
    "Shelf Outer Corner": ItemData(405, ItemClassification.useful),
    "Shelf Quad": ItemData(406, ItemClassification.useful),
    "Shelf Quad Half": ItemData(407, ItemClassification.useful),
    "Fridge Single": ItemData(408, ItemClassification.useful),
    "Double Fridge": ItemData(409, ItemClassification.useful),
    "Fridge Mini": ItemData(410, ItemClassification.progression),
    "Display Fridge Single": ItemData(411, ItemClassification.useful),
    "Display Fridge Double": ItemData(412, ItemClassification.useful),
    "Single Freezer": ItemData(413, ItemClassification.progression),
    "Freezer": ItemData(414, ItemClassification.useful),
    "Triple Freezer": ItemData(415, ItemClassification.useful),
    "Checkout Counter": ItemData(416, ItemClassification.progression),
    "Checkout Counter Mirrored": ItemData(417, ItemClassification.useful),
    "Small Rack": ItemData(418, ItemClassification.progression),
    "Tall Rack": ItemData(419, ItemClassification.useful),
    "Spot Light": ItemData(420, ItemClassification.useful),
    "Self Checkout Counter": ItemData(421, ItemClassification.useful),
    "Self Checkout Counter Mirrored": ItemData(422, ItemClassification.useful),
    "Speaker": ItemData(423, ItemClassification.useful),
    "Category Sign": ItemData(424, ItemClassification.useful),
    "Security Camera": ItemData(425, ItemClassification.useful),
    "Security Antenna": ItemData(426, ItemClassification.useful),
    "Scale": ItemData(427, ItemClassification.progression),
    "Produce Stall": ItemData(428, ItemClassification.progression),
    "Trash Can": ItemData(429, ItemClassification.useful),


    # === FILLERS (Offset 500+) ===
    "Money Boost": ItemData(501, ItemClassification.filler),
    "XP Boost": ItemData(502, ItemClassification.filler),
    "Blackfriday": ItemData(503, ItemClassification.filler),

    # === TRAPS (Offset 700+) ===
    "Tax Audit Trap": ItemData(701, ItemClassification.trap),
    "Dust Storm Trap": ItemData(702, ItemClassification.trap),
    "Power Outage Trap": ItemData(703, ItemClassification.trap),
    "Trash Flood Trap": ItemData(704, ItemClassification.trap),
    "Expired Products Trap": ItemData(705, ItemClassification.trap),
    "Robbery Trap": ItemData(706, ItemClassification.trap),

    # === STORAGE ROOM UPGRADES (Offset 800+) ===
    PROGRESSIVE_STORAGE_ITEM: ItemData(801, ItemClassification.progression),

    # === LOAN AUTHORIZATIONS (Offset 850+) ===
    **{f"Loan Authorization {i}": ItemData(850 + i, ItemClassification.useful) for i in range(1, 7)},
}

dlc_licenses = {
    "dlc_clothing": {
        "License 51": ItemData(151, ItemClassification.progression),
        "License 52": ItemData(152, ItemClassification.progression),
        "License 53": ItemData(153, ItemClassification.progression),
        "Metal Hanger": ItemData(430, ItemClassification.progression),
        "Wooden Hanger": ItemData(431, ItemClassification.useful),
        "Wooden Pegboard Display": ItemData(432, ItemClassification.progression),
        "Metal Pegboard Display": ItemData(433, ItemClassification.useful),
    },
    "dlc_electronics": {
        "License 54": ItemData(154, ItemClassification.progression),
        "License 55": ItemData(155, ItemClassification.progression),
        "License 56": ItemData(156, ItemClassification.progression),
        "Wooden Pegboard Display": ItemData(432, ItemClassification.progression),
        "Metal Pegboard Display": ItemData(433, ItemClassification.useful),
    },
    "dlc_hardware": {
        "License 57": ItemData(157, ItemClassification.progression),
        "License 58": ItemData(158, ItemClassification.progression),
        "License 59": ItemData(159, ItemClassification.progression),
        "Wooden Pegboard Display": ItemData(432, ItemClassification.progression),
        "Metal Pegboard Display": ItemData(433, ItemClassification.useful),
    },
    "dlc_essentials": {
        "License 60": ItemData(160, ItemClassification.progression),
        "License 61": ItemData(161, ItemClassification.progression),
        "License 62": ItemData(162, ItemClassification.progression),
        "License 63": ItemData(163, ItemClassification.progression),
        "License 64": ItemData(164, ItemClassification.progression),
        "License 65": ItemData(165, ItemClassification.progression),
    },
    "dlc_bakery": {
        "License 66": ItemData(166, ItemClassification.progression),
        "License 67": ItemData(167, ItemClassification.progression),
        "License 68": ItemData(168, ItemClassification.progression),
        "Progressive Baker": ItemData(901, ItemClassification.progression),
        "Single Bakery Shelf": ItemData(435, ItemClassification.progression),
        "Bakery Shelf": ItemData(436, ItemClassification.useful),
        "Oven": ItemData(434, ItemClassification.progression),
    },
    "dlc_ice_cream": {
        "License 69": ItemData(169, ItemClassification.progression),
        "Progressive Ice Cream Helper": ItemData(911, ItemClassification.progression),
        "Ice Cream Stand": ItemData(437, ItemClassification.progression),
    },
    "dlc_vending": {
        "Vending Machine Slot": ItemData(920, ItemClassification.useful),
    }
}

for dlc_key, licenses in dlc_licenses.items():
    item_table.update(licenses)


# For validation in options.py, to ensure that the user doesn't select invalid values
ALL_LICENSES = [name for name in item_table.keys() if name.startswith("License")]
ALL_FURNITURE = [
    name for name, data in item_table.items() 
    if 400 <= data.id < 500
]
ALL_VEHICLES = ["Skateboard", "Bicycle", "Scooter", "Sedan", "Pickup Truck"]
ALL_TRAPS = [name for name in item_table.keys() if name.endswith("Trap")]

# Only these item names drive the percentage gates on Day and Store Level
# locations.  This is deliberately a whitelist rather than a classification
# lookup: furniture and any future progression items must not silently change
# the milestone logic.
DAY_AND_LEVEL_PROGRESSION_ITEMS = frozenset({
    *ALL_LICENSES,
    PROGRESSIVE_SECTION_ITEM,
    PROGRESSIVE_STORAGE_ITEM,
    *PROGRESSIVE_STAFF_COUNTS,
    *(item_name for staff in PROGRESSIVE_DLC_STAFF_COUNTS.values() for item_name in staff),
})
