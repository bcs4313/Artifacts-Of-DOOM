# The Artifacts Of Doom

Adds 5 customizable artifacts to traumatize your friends with.

![Picture marking your imminent death](https://cdn.discordapp.com/attachments/613179339269210149/974067165730832465/trauma.jpg)

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
* All items are shared amongst players, but enemies are much stronger.

**Artifact Of The Titans**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266993281953873/Revised_-_Icon_2._Artifact_of_the_Titans.png align="center" width="85px" height="85px"/>
* Enemy size scales according to their credit cost, and elites are scaled even more brutally.
* Teleporter Boss Scales multiplicatively with stage (max 10 stages)

**Artifact of Smash**&nbsp;
<img src=https://cdn.discordapp.com/attachments/613179339269210149/976266916450689044/Revised_-_Icon_5._Artifact_of_Smash.png align="center" width="85px" height="85px"/>
* Enemies and Survivors are knocked back in proportion to their remaining health and damage dealt.
* Impacts on surfaces deal damage to all entities. 
* Prepare to SMASH, or be SMASHED!
<br>
<br>
New artifacts and updates to current ones are in development. 
I am open to any feedback and/or requests for new features
on the RoR2 discord @bcs4313, or through my email at bcs4313@g.rit.edu. Also I would love to watch you play it (yes I am a sadist)!

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
```

Currently Planned Features: 
* A video demonstrating the 5 artifacts, and what you can do with them.
* Getting golems to resize without a strange visual bug...
* Fixing a visual bug with Artifact of War's healthbars client-side.
* Stabilization and bug fixes for all artifacts (we are getting there).
* More outcomes for the entropy artifact.
* Thinking of a new artifact...
