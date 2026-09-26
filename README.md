# Pac-Man

A C#/GTK# remake of the classic Pac-Man, complete with four ghosts (each with their own release timing), a maze with walls that toggle on and off, teleporters, and speed-altering power-ups.

## Features

- **Classic Pac-Man gameplay** — eat all the score circles in the maze while avoiding four ghosts (Blinky, Pinky, Inky, Clyde)
- **Randomized ghost AI**, with each ghost released from the center box at a set time
- **Scoring** — 10 points per score circle, 100 points per ghost eaten, no score cap
- **Lives system** — two lives to start; losing all of them ends the game
- **Dynamic maze elements** — a pair of walls near the speed-boost power-ups appear and disappear every 5 seconds
- **Screen wraparound** — moving off one vertical edge of the maze brings you out the other side
- **Four power-ups**:
  | Power-up | Effect |
  |---|---|
  | Phase | Disables wall collisions, letting Pac-Man pass through walls (ending it while inside a wall is fatal) |
  | Slow Down | Halves Pac-Man's speed |
  | Speed Up | Significantly increases Pac-Man's speed |
  | Teleporter | A pair of portals that send Pac-Man to each other's location; both reappear 10 seconds after either is used |
- **Title, gameplay, and game-over screens**, each with its own background and countdown at the start of a round
- Sound effects for eating circles, eating ghosts, power-ups, and game-start/win

## Requirements

- .NET SDK (6.0 or later)
- GTK# (`Gtk`, `Gdk`, `Cairo` bindings) and the native GTK3 runtime installed on your OS
- [NAudio](https://www.nuget.org/packages/NAudio) for sound playback

## Setup

```bash
dotnet new console -o PacMan
# copy Program.cs into the PacMan folder, replacing the generated one
cd PacMan
dotnet add package NAudio
dotnet add package GtkSharp
```

GTK# wraps the native GTK library, so the GTK3 runtime must also be installed separately (e.g. the GTK for Windows Runtime installer on Windows, or your package manager's `gtk3` package on Linux/macOS) or the game will build but crash on launch.

The game also expects its asset folders alongside the executable:

```
graphics/
├── sprites/
│   ├── pacman/      # pacManRight.png, pacManLeft.png, pacManUp.png, pacManDown.png
│   ├── blinky/, inky/, clyde/, pinky/   # <name>Right/Left/Up/Down.png each
│   └── ghostAfraid.png
├── powerUps/         # speedUp.png, slowDown.png, phase.png, teleporter.png
└── backgrounds/      # pacManTitle.png, gameWon.png, gameOver.png
sounds/
├── pacManBeginning.wav
├── pacManChomp.wav
├── eatGhost.wav
├── speedUp.wav
├── slowDown.wav
├── phase.wav
├── portal.wav
└── gameWon.wav
```

## Running the Game

```bash
dotnet run
```

## Controls

| Key | Action |
|---|---|
| `Spacebar` | Start the game from the title screen |
| `Up Arrow` | Move up |
| `Down Arrow` | Move down |
| `Left Arrow` | Move left |
| `Right Arrow` | Move right |

## How to Play

Press spacebar on the title screen to begin. A three-second countdown plays before movement starts. Guide Pac-Man through the maze, eating score circles for 10 points each while dodging the four ghosts, which are released one at a time in the order Blinky, Pinky, Inky, Clyde and move with randomized AI. Getting caught by a ghost costs a life; running out of lives ends the game in a loss. Eating every score circle wins the game — there's no score limit, so the score only exists to track and compare your performance.

Power-ups scattered around the maze grant temporary abilities and, aside from the teleporters, don't respawn once used, so it's worth saving them for when you need them. Two of the power-ups sit behind walls that appear and disappear every 5 seconds, so timing your route matters. Moving off one vertical edge of the maze brings you out the other side.

## Project Structure

- `Program.cs` — full game source: sprite movement and collision, ghost AI, power-ups, maze layout, scoring, and the main game window/loop
