# The Artifacts Of Doom

Adds 6 customizable artifacts to traumatize your friends with.

![Picture marking your imminent death](https://cdn.discordapp.com/attachments/519358395661156353/1126339595710242816/20221203003816_1.jpg)

# Artifacts Added

**Artifact Of War**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976267041520623666/Revised_-_Artifact_of_War_Enabled.png align="center" width="85px" height="85px"/>
* Time Increase = ++Spawn Rates, +++Difficulty<br> 
(swarm effect +1 every 5 minutes, max 5), Spawnrate Cap = 250
* Stage Increase = ++Monster Evolution +++ Item Droprate<br>
Items are exponentially given to monsters, items dropped 
are automatically given to the player, with a chat notification.
* This artifact generally creates a harder version of the game.<br> 
Consider rainstorm to be monsoon difficulty, monsoon is just terrifying, drizzle == ??

**Artifact Of Entropy**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266895751806976/Revised_-_Icon_4._Artifact_of_Entropy.png align="center" width="85px" height="85px"/>
* Various actions in-game are randomly given effects from a large pool of outcomes ranging from being a godsend to single handedly destroying everyone you love.
* These outcomes have a randomly defined chance to occur, from 0-100%.
* Discovering what actions do what, adapting, and dealing with mental health trauma is vital to your survival. Will you master the chaos? Or die trying...
* Current Amount of possible outcomes to experience: 80
* Current Amount of possible actions that cause outcomes: 23

**Artifact Of Unity**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266868060983316/Revised_-_Icon_3._Artifact_of_Unity.png align="center" width="85px" height="85px"/>
* All items are shared among players, but enemies are much stronger.

**Artifact Of The Titans**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266993281953873/Revised_-_Icon_2._Artifact_of_the_Titans.png align="center" width="85px" height="85px"/>
* Enemy size scales according to their credit cost, and elites are scaled even more brutally.
* Teleporter Boss Scales multiplicatively with stage (max 10 stages)

**Artifact of Smash**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266916450689044/Revised_-_Icon_5._Artifact_of_Smash.png align="center" width="85px" height="85px"/>
* Enemies and Survivors are knocked back in proportion to their remaining health and damage dealt.
* Impacts on surfaces deal damage to all entities. 
* Prepare to SMASH, or be SMASHED!

**Artifact of Reconstruction**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/1133572478518698054/ArtifactOfMetamorphosis.png align="center" width="85px" height="85px"/>
* Play as any monster in the game, with any base stats of your choosing.
* You can open the ui for selecting your monster in-game through a keybind (default: F2)
* Perfect for mithrix gamers like myself.

<br>
<h4>New artifacts and updates to current ones are in development. 
I am open to any feedback and/or requests for new features
on the RoR2 discord @bcs4313, or through my email at bcs4313@g.rit.edu. Also I would love to watch you play it (yes I am a sadist)!</h4>


## Changelog
```{r, max-height='10px'}
1.0.0

* Mod Release

1.0.1
* Fixed UI Bug with item/artifact descriptions not appearing sometimes in a run.
* Artifact icon display order adjusted.

1.1.3
* Added customizability for the Artifact of Entropy, Artifact of War and Artifact of The Titans (new RiskOfOptions dependency required).
* Printers, Scrappers, and other item removers now work with the Artifact of Unity.
* Increased general mod compatibility / bugfixing

**1.1.5**
* Fixed an issue where Artifact of Titans would make monsters without a cost invisible.
* Monsters that scale are no longer partially in the ground.
* Added fixed scaling settings to monsters and bosses.

1.1.6
* Fixed Artifact of Entropy not working for literally no discernable reason.

1.3.0
* Prevented lunar portals, Gold Shore portals, gold shrines, void seeds, and other unnecessary/conflicting dupes from being created in Artifact of War.
* Updated Artifact Icons to have their own unique look and style.
* Added a new artifact: Artifact of Smash.
* Added more customizability to Artifact of War.
* The monster item pool for Artifact of War has been increased.

1.4.0
* Added 20 outcomes to The Artifact of Entropy, totaling to 80 outcomes.
* Artifact of Entropy runs can be seeded and quickly shared via clipboard copy/paste, for exact copies of other entropy runs (check the Risk of Options settings for more information).
* Fixed the probability offset and multiplier not working for the Artifact of Entropy! -_-
* Void items are now supported for the Artifact of Unity.

1.4.4
A metric ton of bugfix/tuning/quality of life updates.
* Added default options to all artifacts with custom settings.
* Monsters/items from gangs in artifact of entropy are now consistent with their given seed value. 
* The Artifact of Entropy now has a custom setting to generate a new set of events upon heading into the next stage. 
* fixed a common outOfRangeException for the findRandomMonster() supporting method.
* Some triggers in artifact of entropy are limited in events due to their inability to supply a player body, however, a workaround was made and now they can activate any event.
* Triggers affected -> Teleporter, CoinBarrel, Item Drop
* Some triggers strictly targeted the server host, even when a client player would do an action. This is no longer an issue.
* Triggers affected -> W, A, S, D, Ability Triggers 
* Fixed a ton of null exceptions everywhere.
* Fixed a bug where events that played sounds to all players would bug out if the host is dead and the client is not.
* Fixed a synchronization issue with titans that makes some enemies fail to resize. This is due to packet loss and higher ping levels. The system is not FOOLPROOF, but is relatively consistent.
* Some adjustments to Artifact Of Entropy's code have reduced its lagginess by a large margin.

1.5
* Major overhaul to how Artifact of Smash functions
* Drastically decreased crashes from the artifact, especially for hitting wisps.
* Increased Impact Threshold for damaging players
* Enemy / Survivor health doubled
* Added exponential knockback scaling based on missing health before the hit
* Enemies dying from impacts now reward players for the kill
* Changed knockback formula to reduce lag
* Added explosion animation for players dying from impact.
* Added settings to adjust knockback and damage levels for non-boss enemies, bosses, 
and players separately.

1.5.1
* Fixed a sus bug from Artifact of Entropy.

1.5.3
* Added a feature that resynchronizes lost items if a player dies with Artifact of Unity on. The setting can be toggled on/off.

1.6
* Added a new artifact: Artifact of Reconstruction.
* Golem bug fixed from Artifact of Titans.
* Fixed health desynchronization from Artifact of War that would bug out healthbars.
* Boosted health of bosses in Artifact of War to make them more balanced.
* Added option to omit messages from Artifact of War and Entropy.
* Fixed Artifact of Unity item dupe bug.
* General stability changes / polish.

1.6.2
* Artifact of Reconstruction:
* Migrated stat changes to the in-game UI. 
* Stat changes are now client-side compatible I.E anyone can choose any stats/morph they want.
* Added elite modifier options to in-game UI. The modifiers can stack.
* Added drones as options for morph selection.
* Artifact of War:
* Completely rebalanced artifact to reduce problems caused by the exponential growth of items for monsters and players.
* Added health and damage growth parameters in RoROptions to tweak difficulty.
* Added preset to change Artifact of War to pre-rebalance settings, this is called 'legacy mode.'
* If any part of Artifact of War still feels unbalanced, feel free to report it to me in Github.
```

Currently Planned Features: 
* A new artifact (WIP as of 8/23/23)! It will be void themed.
* Some QOL fixes / tweaks.
* Debug / Compatibility improvements.
* More outcomes for the entropy artifact.
* I need more feature ideas / artifact ideas! See my github to make a request.
* A video demonstrating the 6 artifacts, and what you can do with them..
