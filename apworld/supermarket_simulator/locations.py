from typing import Dict, NamedTuple

class LocationData(NamedTuple):
    id: int

# Base IDs 
STORE_LEVEL_BASE_ID = 200000
DAYS_COMPLETED_BASE_ID = 210000
STORAGE_ROOM_BASE_ID = 220000
SECTION_ROOM_BASE_ID = 225000
MONEY_MILESTONE_BASE_ID = 230000


location_table: Dict[str, LocationData] = {}

# Pre-generate all possible Store Level locations up to 200
for level in range(1, 201):
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