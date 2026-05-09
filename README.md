# Once More, With Squealing!

A 2-player local multiplayer top-down 2.5D racing game made for TOJam.

## Theme

This game is based on the TOJam themes:

- **Winning is for Losers**
- **Once More, With Feeling!**

Two cute lab rats race through the same chaotic laboratory course again and again. The first rat to reach the finish line wins the round and earns a point.

But winning comes with a price.

After each round, the losing player chooses a hazard card that punishes the winner in the next round. The better you do, the more cursed you become. Losing gives you power, revenge, and emotional damage.

First player to reach 10 points wins the match.

## Game Concept

Players control two cartoon lab rats trapped in a bright, sterile, low-poly laboratory test chamber. Scientists are running questionable experiments, and the rats are forced to race repeatedly through the same obstacle course.

The game uses a **top-down angled camera**, looking down at the players from above. The environment is built in **3D low-poly**, while the rats are represented as cute **2D cartoon sprites/billboards** moving through the 3D test chamber.

Each round is short, fast, and chaotic. The level resets after every race, but new hazards and troll effects make each retry more ridiculous.

The core joke:

> Winning gives you points. Losing gives you power.

## Gameplay Loop

1. Both players start at the beginning of the lab course.
2. They race toward the finish line.
3. The first player to reach the finish wins the round and gains 1 point.
4. The losing player chooses from 3 random hazard cards.
5. That hazard punishes the winner in the next round.
6. Optionally, the winner may choose a weaker troll card to annoy the loser.
7. The level resets.
8. The next round begins with the selected effects active.
9. First player to 10 points wins the match.

## Controls

### Player 1

| Action | Key |
|---|---|
| Move | WASD |
| Jump / Hop | Space |

### Player 2

| Action | Key |
|---|---|
| Move | Arrow Keys |
| Jump / Hop | Right Ctrl |

Controls may be adjusted depending on the final Unity input setup.

## Features

- 2-player local multiplayer
- Top-down 2.5D platform racing
- Angled camera looking down at the players
- Cute 2D lab rat characters in a 3D low-poly lab
- Short replayable rounds
- First-to-10 scoring system
- Loser-powered hazard card system
- Optional winner troll cards
- Funny lab experiment theme
- Chaotic but readable obstacle course

## Hazard Cards

Hazard cards are chosen by the losing player and are used to punish the winner in the next round.

Possible hazards include:

- **Slippery Floor Trial**  
  The winner has slippery ice physics.

- **Heavy Rat Serum**  
  The winner has reduced jump height.

- **Tiny Legs Injection**  
  The winner moves slower.

- **Overcaffeinated Rat**  
  The winner moves faster but has terrible braking.

- **Falling Cheese**  
  Cheese blocks fall near the winner.

- **Shock Collar Calibration**  
  The winner must react to timed shock warnings.

- **Fake Cheese**  
  Bait cheese knocks the winner backward.

- **Lab Fan Malfunction**  
  Wind pushes the winner during jumps.

- **Glass Floor Anxiety**  
  Platforms crack faster under the winner.

- **Cheese Tax**  
  The winner must collect cheese before finishing.

## Troll Cards

Troll cards are weaker effects chosen by the round winner to annoy the loser. These are meant to be funny, not game-breaking.

Possible troll cards include:

- **Squeaky Paws**  
  The loser makes squeaky sounds while moving.

- **Cone of Shame**  
  The loser wears a silly lab cone.

- **Scientist Laugh Track**  
  A laugh track plays when the loser messes up.

- **Name Tag of Shame**  
  The loser gets a floating embarrassing label.

- **Tiny Violin**  
  Sad music plays near the loser.

- **Delayed Release Door**  
  The loser starts slightly late for a brief moment.

## Visual Style

The game uses a playful lab experiment aesthetic:

- Bright white sterile laboratory
- Low-poly 3D test chamber
- Top-down angled camera view
- Cute 2D cartoon lab rat sprites/billboards
- Ramps, platforms, glass floors, and trap tiles
- Cheese-based hazards
- Warning signs and flashing lights
- Scientist monitors and silly UI commentary
- Clean, readable, chaotic visuals

## Design Goals

- Easy to understand within 10 seconds
- Funny to play with friends
- Short rounds, around 20–40 seconds each
- Hazards should be annoying but fair
- Troll effects should be emotional and silly, not overpowered
- The same level should feel different every round
- The game should clearly communicate the joke: winning is dangerous

## Technical Overview

Built in Unity as a top-down 2.5D racing platformer.

Core systems include:

- Two-player local input
- Top-down movement through a 3D course
- Angled camera looking down at the players
- 2D rat sprites or billboards inside a 3D environment
- Jumping / hopping over obstacles
- Finish line detection
- Round reset system
- Score tracking
- First-to-10 win condition
- Card selection UI
- Hazard and troll effect application
- Simple low-poly lab environment

## Project Scope

This project is designed for a 2-day game jam, so the focus is on a small but polished core loop:

- One short race level
- Two playable rats
- A working round system
- A handful of functional hazard cards
- Simple but funny UI
- Strong theme execution

Extra polish such as more cards, animations, sound effects, and scientist commentary can be added if time allows.

## How to Play

1. Launch the game.
2. Choose local 2-player mode.
3. Race to the finish line.
4. Win rounds to gain points.
5. Lose rounds to curse your opponent.
6. Survive the chaos.
7. First rat to 10 points wins.

## Credits

Created for TOJam.

Game concept inspired by chaotic couch multiplayer games, silly lab experiments, cartoon rat energy, and the emotional pain of being punished for being good at the game.
