﻿# LobotomyAutomation

## Features

1. Press `Shift + click` to repeat the work.
2. Press `Ctrl + click` to repeat the work until the agent gets the EGO gift.
3. Press `Alt + click` to repeat the work until the agent gets max EXP of that work type.
5. Press `P` to pause the automation works.
6. Press `V` to clear all automation works.
7. Angela will diss you when you made a wrong decision that will result in agents die or creatures escape. Only support a few creatures.

## Testing Features

1. Automatically handle Qliphoth Meltdown events.
2. Automatically suppress `Morning` and `Lunch` ordeal creatures.

## Incoming Features

- Farm mode. Press `F` on a creature to automatically pick agents who hasn't its EGO gift to work with it.

## Automation Strategy

Agents will not repeat the work work if:

1. The agent is not available.
   - HP/Mental is not full, or not in `IDLE` status.

2. The agent is not capable of doing that work. Usually because it will result in an instant death or creature escape.
   - For example, he is the last agent who worked with Teddy Bear.

3. The agent doesn't have enough confidence to survive from the work.

## Supported creatures

- Beauty Beast
- Big Bird
- Dream of Black Swan
- Funeral of the Dead Butterflies
- 「CENSORED」
- Clouded Monk
- Don't Touch Me
- Fire Bird (Not recommend)
  - Better if you had a low-level agent in its dept.
- Happy Teddy Bear
- The Queen of Hatred (Not recommend)
  - Better if you had a super ultimate double plus work success rate agent in its dept.
- Judgement Bird
- Laetitia
- The Little Prince
- Naked Nest
- Nothing
- Queen Bee
- Red Shoes
- Today's Shy Look
- Singing Machine
- Snow White's Apple