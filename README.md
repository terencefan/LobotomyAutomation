# LobotomyAutomation
[简体中文](./README-zh-CN.md)

Here are other mods that published by me, which could be useful when using this mod (but not required)

[Today's Ordeal](https://www.nexusmods.com/lobotomycorporation/mods/127)
[Relocate Agents](https://www.nexusmods.com/lobotomycorporation/mods/128)

## Features

1. **Angela will diss you when you made a wrong decision that will result in agents dying or creatures escaping**
   - Note that I haven't finished the translation works now, only Chinese supported.
3. Press `Shift + click` to repeat the work.
4. Press `Ctrl + click` to repeat the work until the agent gets the EGO gift.
5. Press `Alt + click` to repeat the work until the agent gets max EXP of that work type.
6. Press `T` when selecting a creature to let Angela help you find candidates who don't have the EGO gift to work with the creature.
   - Press `T` again to toggle it off.
9. Press `Shift` when selecting agents to cancel their automation works.
10. Press `P` to pause all automation works.
11. Press `V` to clear all automation works.
10. Automatically handle Qliphoth Meltdown events.
11. Automatically suppress `Dawn` and `Noon` ordeal creatures.

### Next Features

1. Automatically suppress escaped creatures.
1. Automatically rescue agents that are in **CANNOT_CONTROL** or **PANIC** status.

## Automation Strategy

Agents will not repeat the work work if:

1. The agent is not available.
   - HP/Mental is not full, or not in `IDLE` status.

2. The agent is not capable of doing that work. Usually because it will result in an instant death or creature escape.
   - For example, if the agent was the last agent who worked with Teddy Bear, he won't repeat the work.

3. The agent doesn't have enough confidence to survive from the work.
   - Current threshold is 99%.

## Work priority

1. Handle urgent events. From riskLevel 5 to 1.
2. Supress escaping creatures.
3. --- return if in emergency (ordeal) ---
4. Try running macros (Agent repeat work)
5. Try farming

## Supported anomoly creatures

### ![color](https://via.placeholder.com/15/1df900/000000?text=+) ZAYIN
- All

### ![color](https://via.placeholder.com/15/13a2ff/000000?text=+) TETH
- All

### ![color](https://via.placeholder.com/15/fff900/000000?text=+) HE
- All

### ![color](https://via.placeholder.com/15/7B2BF3/000000?text=+) WAW
- All

### ![color](https://via.placeholder.com/15/ff0000/000000?text=+) ALEPH
- All except for Plague Doctor
