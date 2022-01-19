# LobotomyAutomation
[简体中文](./README-zh-CN.md)

## Features

1. **Angela will diss you when you made a wrong decision that will result in agents die or creatures escape.**
1. Press `Shift + click` to repeat the work.
2. Press `Ctrl + click` to repeat the work until the agent gets the EGO gift.
3. Press `Alt + click` to repeat the work until the agent gets max EXP of that work type.
4. Press `T` when selecting an creature to let Angela help you find candidates who don't have the EGO gift to work with the creature.
   - Press `T` again to toggle it off.
6. Press `H` to send all agents back to their restroom.
   - Press `Alt + H` to send all agents to the passage next to their restroom. Can be useful when handling Outter God Noon.
   - Press `Shift + H` to send all agents to the top left elevator. Can be useful when handling Machine Midnight.
9. Press `Shift` when selecting agents to cancel their automation works.
10. Press `P` to pause all automation works.
11. Press `V` to clear all automation works.
1. Automatically handle Qliphoth Meltdown events.
3. Automatically suppress `Morning` and `Lunch` ordeal creatures.

### TODO Features

1. Automatically suppress escaped creatures.
1. Rescue agents that are in `CANNOT_CONTROL` and `PANIC` status.

## Automation Strategy

Agents will not repeat the work work if:

1. The agent is not available.
   - HP/Mental is not full, or not in `IDLE` status.

2. The agent is not capable of doing that work. Usually because it will result in an instant death or creature escape.
   - For example, if the agent was the last agent who worked with Teddy Bear, he won't repeat the work.

3. The agent doesn't have enough confidence to survive from the work.
   - Current threshold is 99%.

## Supported creatures

### ZAYIN
- Don't Touch Me

### TETH
- Beauty Beast
- Bloodbath
- Forsaken Murderer
- Grave of Cherry Blossoms
- Scorched Girl
- Today's Shy Look
- Void Dream

### HE
- Funeral of the Dead Butterflies
- Der Freischütz
- Happy Teddy Bear
- Laetitia
- Nameless Fetus
- Red Shoes
- Singing Machine
- Snow Queen

### WAW
- Big Bird
- Dream of Black Swan
- Clouded Monk
- The Dreaming Current
- Fire Bird (Not recommend)
  - Better if you had a low-level agent in its dept.
- The Queen of Hatred (Not recommend)
  - Better if you had a super ultimate double plus work success rate agent in its dept.
- Judgement Bird
- The Little Prince
- Naked Nest
- Queen Bee
- Snow White's Apple

### ALEPH
- 「CENSORED」
- Nothing
