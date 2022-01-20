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

9. Press `Shift` when selecting agents to cancel their automation works.
10. Press `P` to pause all automation works.
11. Press `V` to clear all automation works.
1. Automatically handle Qliphoth Meltdown events.
3. Automatically suppress `Dawn` and `Noon` ordeal creatures.

### Next Features

1. Automatically suppress escaped creatures.
1. Rescue agents that are in **CANNOT_CONTROL** and **PANIC** status.
2. ress `Alt + H` to send all agents to the passage next to their restroom. Can be useful when handling Outter God Noon.
3. Press `Shift + H` to send all agents to the top left elevator. Can be useful when handling Machine Midnight.

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
- Don't Touch Me
- Fairy Festival
- One Sin and Hundreds of Good Deeds
- Opened Can of Wellcheers

### ![color](https://via.placeholder.com/15/13a2ff/000000?text=+) TETH
- Beauty Beast
- Bloodbath
- Forsaken Murderer
- Grave of Cherry Blossoms
- Scorched Girl
- Today's Shy Look
- Void Dream

### ![color](https://via.placeholder.com/15/fff900/000000?text=+) HE
- Funeral of the Dead Butterflies
- Der Freischütz
- Happy Teddy Bear
- Laetitia
- Nameless Fetus
- Red Shoes
- Singing Machine
- Snow Queen 
  - Agent freezing will be treated as **Urgent**

### ![color](https://via.placeholder.com/15/7B2BF3/000000?text=+) WAW
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

### ![color](https://via.placeholder.com/15/ff0000/000000?text=+) ALEPH
- 「CENSORED」
- Nothing
- White Night
  - Qliphoth counter < 3 will be treated as **Urgent**
  - Farm with this creature is risky since you may not have enough success rate to raise it's Qliphoth counter before reaches next meltdown level.
