# BetterTimeStop
A Bopl mod that makes it so time stop lasts for how long it is charged

*Adjusts the Timestop ability to have a duration for as long as its charged for*

## Quick Links
* **[MyBoplMods Repo](https://github.com/maxgamertyper/MyBoplMods)**
* **[Youtube Video](https://youtu.be/pQEt4F5tMKU)**
* **[Direct Video Download](https://github.com/maxgamertyper/BetterTimeStop/blob/main/BetterTimeStop.mp4)**
* **[Thunderstore Link](https://thunderstore.io/c/bopl-battle/p/maxgamertyper1/BetterTimeStop/)**

> *The Youtube video doesn't include the full capabilites of the mod as that video was recorded with V1.0.0*

---

## General Information & Setup

### Mod-Manager Setup
> *this is specifically guided for thunderstore, it may be slightly different for other mod managers*

#### Prerequisites
* A mod-manager (thunderstore, R2modman, or others) configured for the game bopl battle
* The game Bopl Battle

#### Steps

1) access the **Bopl Battle** game
2) make a new mod profile
3) go to the mods tab
4) search for "BetterTimeStop"
5) click download
6) run the game twice (it won't work the first time as the manager is initializing the mod installer)
7) check your mod manager's config area to change the time stop maximum and minimum duration
8) have fun


### BepInEx Setup
> *note: this is directed towards a windows installation*

#### Prerequisites
* An installation of the BepInEx Zip file
* the game Bopl Battle
* the BetterTimeStop.dll file

#### Steps
1) find your game directory through steam likely at `C:\Program Files (x86)\Steam\steamapps\common\Bopl Battle`
2) unzip the BepInEx file into the folder
3) run the game once
4) return to the directory
5) move the BetterTimeStop.dll file into the plugins folder
6) run the game
7) check the BepInEx config directory for the config file
8) change the time stop maximum and minimum durations
9) have fun

---

## Configuration Architecture

The Configuration Pipeline for this mod determines CIL injection priority and location:
* **MinimumDuration:** This changes the minimum duration of the timestop and injects that constant into the CIL code
* **MaximumDuration:** This changes the maximum duration of the timestop and injects that constant into the CIL code in addition to a tracker function that times ability usage time
