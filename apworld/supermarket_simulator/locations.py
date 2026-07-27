from typing import Dict, NamedTuple
from .items import item_table

class LocationData(NamedTuple):
    id: int

# Base IDs 
STORE_LEVEL_BASE_ID = 200000
DAYS_COMPLETED_BASE_ID = 210000
STORAGE_ROOM_BASE_ID = 220000
SECTION_ROOM_BASE_ID = 225000
MONEY_MILESTONE_BASE_ID = 230000
LICENSE_PURCHASE_BASE_ID = 240000
CUSTOMER_CHECKOUT_BASE_ID = 250000


location_table: Dict[str, LocationData] = {}

# Pre-generate all possible Store Level locations (starting at Level 2 up to 200)
for level in range(2, 201):
    location_table[f"Store Level {level}"] = LocationData(STORE_LEVEL_BASE_ID + level)

# Pre-generate all possible Day Completed locations up to 1000
for day in range(1, 1001):
    location_table[f"Day {day} Completed"] = LocationData(DAYS_COMPLETED_BASE_ID + day)

# Pre-generate Storage Room Upgrade locations
for upgrade in range(1, 21):
    location_table[f"Storage Room Upgrade {upgrade}"] = LocationData(STORAGE_ROOM_BASE_ID + upgrade)

for upgrade in range(1, 33):
    location_table[f"Section Room Upgrade {upgrade}"] = LocationData(SECTION_ROOM_BASE_ID + upgrade)

# Pre-generate Money Milestone locations
for money in range(1000, 500001, 1000):
    location_table[f"Earn {money}$"] = LocationData(MONEY_MILESTONE_BASE_ID + (money // 1000))

# Pre-generate License Purchase locations for all licenses in item_table
for lic_name, item_data in item_table.items():
    if lic_name.startswith("License "):
        lic_id = item_data.id - 100
        location_table[f"Purchase {lic_name}"] = LocationData(LICENSE_PURCHASE_BASE_ID + lic_id)

# Pre-generate Customer Checkout locations (1 to 100)
for count in range(1, 101):
    location_table[f"Customer Checkout {count}"] = LocationData(CUSTOMER_CHECKOUT_BASE_ID + count)