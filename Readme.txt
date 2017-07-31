NOOSE Mod (LCPDFR Edition) v0.1
by Naruto 607

This script is a contribution to LCPDFR by adding new features of terrorist threat and other crime things.
It is based from GTAIV NooseMod 0.1 BETA by _hax which had been left for years and still under beta.
Much of bugs are introduced, and yet it is only working for GTA IV version 1.0.4.0 and lower, which current LCPDFR version does not support it.

This is the first script mod I made from my personal learning of C# programming by the book and scripting tutorials over the internet.
It will get updated to a full perfection if any bugs found.


Description:
====
Join elite NOOSE unit to fight against terrorists in Liberty City!
Fulfill your duty during many breathtaking missions.
Basically you have one objective: eliminate every terrorists in the area.
However, they are not stupid - they will take hostages, so you can't let them get hurt.


Features:
====
1.	Difficulty Settings: NOOSE Mod offers 3 difficulties:
		Easy - Enemies will have full health and will be marked on the map
		Medium - Enemies will have full health + 50% armor and there will be no blips
		Hard - Enemies will have 100% armor and have increased shoot accuracy (combining Medium difficulty ones)
	To choose difficulties, you can walk to the Chief of Police's office room (in the police station), enter the marker, hit the Interaction key and choose one.
	Be note that if you enabled Hardcore mode in LCPDFR Settings, NOOSE Mod settings will treat it as Hard.
	You can also set manually on your own from the NooseMod.ini provided.

2.	Random Callouts: For every missions (starting from Introduction to the finale "The Warehouse") you will be informed via Dispatch callouts.
		You can choose to respond or disregard just like any other callouts.

3.	Progress Tracking and Recording: Your progress will be tracked and recorded to the separated database. Each mission will include:
		Killed suspects
		Killed hostages
		Arrested suspects
		Rescued hostages
		Friendly Officers casualties
		Squad casualties
	After all missions completed, all these stats from each mission are summed.
	Every time you made tracks it will be saved, so don't worry about sudden CTDs during your patrols or after the mission completion.
	However, cash reward is recorded to the database, but if you don't save the game (while off duty, of course), earned cash will be lost. Sorry.
	That's why using AngryAmoeba's Bank Account script is fundamental.

	The database is written to the Ms. Access Database in the NooseMod folder by the name stats.mdb.
	You should be able to open that file using Microsoft Office you have (it is saved as 2002-2003 version, so I think it should work on 2003 and later).
	Your progress can also be called ingame by entering the SHDN Console (by pressing the "`" or "~" button, one between Esc, 1, and Tab keys) and type "ShowNooseModStats" w/o quotes.

4.	Progress Reset: You can be able to reset the progress, either is complete or incomplete, by accessing a terminal at where you pick up the partner. It is marked, so you won't confuse which one.
		Use the displayed keys on screen to choose if you want to accept or deny the reset.
		When the progress is reset, all the stats will be cleared and the mission will start again from Mission 1 by waiting for the Dispatch callout when on duty.

5.	Worldwide and Realistic Interaction: Because this involves terrorist activity, large number of officers will be dispatched as a negotiator while waiting for the SWAT/NOOSE team to arrive.
	This will include: police cars, roadblocks, helicopters (very rare)

6.	Forcing Callouts: Just like Callouts+, WoutersCallouts, SuperVillains, and many others, you can "force" the callout to start a specific mission.
	Be note that you cannot, for example, start Mission 2 until you completed Mission 1, and you cannot start any missions if all missions completed.
	Completed missions cannot be restarted (or forced to start again).

	To force a callout, enter the SHDN Console (by pressing the "`" or "~" button, one between Esc, 1, and Tab keys) and type NooseMod to start.
	If you are running other than a SWAT member, type TerroristPursuit to start a unique police pursuit.

7.	Time-sensitive Response: Missions 2 and 5 offer time-sensitive response for the Dispatch callout to work.
	For example: if the actual beta mod starts Bus Stop mission at 2 o'clock post-midnight precise, this one will start the mission between 1 and 3 (no fade-in and outs, real Dispatch callout).
	More than 3 will be non-functional nevertheless, or a.k.a. no terrorist activities, or only regular callouts, so be sure to respond between that time if you are ready.
	Using Time Lock (Real Time Duration or Sync Game Time to System Time) (Native Trainer) may be helpful in some scenes.
	Be note that during that time, if you receive a callout other than this, you can respond or disregard. It's all yours.

8.	Normal Activites + Mission Waiting: Kinda boring to wait for the mission to get informed via Dispatch.
	If you happen to get bored, don't hesitate to partake the police activities (pursuit, arrest, etc.) OR force the callout to come to you.

9.	Intelligent AI: Terrorists tend to get impatient the longer you respond to the crime scene.
	They will put a bullet on a hostage that will cost you significant cash lost during your mission, if any exists, so be sure that you get there in time and plan your strategy.
	Terrorists are also packed with various weapons and tend to get more accurate the longer you stay in the open and the closer you get.
	By any chances the fighting broke out, hostages will tend to cower or run for cover. Interact with them to evacuate, just like how you do with homeless people.
	When the area is clear, hostages tend to evacuate themselves (or freed automatically) from the crime scene.

	Note: Hostages can reach the safe place if they are outside the combat area, no matter it is surrounded or not surrounded by cops.
		Remember, your job is to eliminate terrorists, not frisking or ID-ing the hostage (there's no time for cop pleasantaries or adding arrest crime stats). Order them to evacuate so they can cooperate with the police.

10.	Normal Pursuit: If there are, at least, 3 or 4 terrorists were alive and when certain condition permits, they will escape and by any chance, steal a vehicle, even a police car.
	Regardless of any difficulties, they will be marked (blipped) on the radar again to ease up the search and pursuit. From here on out, it's all yours: kill or arrest.

11.	Battle Results: The battle results will be shown together with the recorded progress. Once a mission is complete, you will achieve a number of cash as "salary" for your noble service in LC.
	Here's the list of cash gain for the progress you made:

	Killed suspects: +300$ per person
	Killed hostages: -600$ per person
	Arrested suspects: +750$ per person
	Rescued hostages: +450$ per person
	Friendly Officers casualties: -90$ per person
	Squad casualties: -150$ per person

	Always ensure paramedics are around the crime scene to prevent Friendly Officers or Squad casualties from being increased.

	At the end of the battle, usually the scene will end up with many corpses.
	If you have vdH MedicalExaminer/Coroner Mod by Abraxas, you can issue a coroner or METT to clean up the scene before continuing with your patrol duty.
	Scene cleanup does not included with your "salary".

12.	Lethal Force: Terrorists give no mercy to law enforcers. If you have trouble of your squads getting too cocky to shoot them, authorize Lethal Force.

13.	Weapons: In this version, you can not also rearm your guns from the police cruisers, but you can also rearm from Enforcer. For the starter kit you will get a pistol every time you are on duty,
		but, depending on the settings, you will get 2 primary weapons and secondary ammo, or 2 primary weapons and a secondary weapon.
	Terrorist weapons will also be varied and accustomed randomly to have a random atmosphere. If you enabled settings for TBoGT weapons, you will see some terrorists carrying TBoGT weapons.
	It will be disabled automatically if using IV or TLAD regardless of changes made.

14.	Partner System: You will get two additional SWAT members joining you after you get a partner and selected a vehicle. They will be deleted if you are off duty or they are dead.
	If you have no partners and responding to a mission, one additional squad group with 4 NOOSE SWATs will be dispatched in a NOOSE Cruiser, NOOSE Patriot, or Enforcer and following your back once joined.
	When the mission is complete, the squad will go back to station or return to duty.

15.	Swap Criminals to Biker Gang Members: One of my specials in this mod. This turns regular criminals (terrorists) into a bunch of Biker Gang members causing terror.
	It also provides access to additional weapons if you play the LCPDFR in TLAD platform. ATM, 2 famous biker gangs are listed:

	a.	The Lost MC: a biker gang led by Johnny Klebitz as the new President. It has made its reputation of gang assault against many members and Angels of Death.
		Johnny has now established moving outposts to his crews to monitor possible locatons that can be used for assault.
		Known violences: Gang-related Violence, Arms Dealing, Drug Trafficking, Speeding, Drive-by activity
	b.	Angels of Death: a biker gang that currently on a truce with The Lost, now made its steps into a war when Billy went out of jail. Made also assaults to other gang members, and also attracted to Securicar theft.
		Now they are attracted to bank robbery and hostage situation demanding ransom to rich families.
		Known violences: Gang-related Violence, Arms Dealing, Drug Trafficking, Grand Theft Auto

16.	Safe Mode: If you are using too many resources that make your game breaks or having script crashes, Safe Mode allows you to remove ambient police vehicles, officers, and roadblocks (when you arrived at the scene).
	This mode only enables NOOSE Backup that will arrive to the crime scene, and only adds you, your squad, terrorists, and hostages.
	Enabling this will also disable Police Officer casualties count, which will significantly affect your profit to an increased one.

17. DYOM: You can be able to create a new mission by creating a new text file with format xxxyyyy, where
		x = mission number, 3-digit integer with max number of 999, start with two zeros if a number, a zero if two numbers, none if three
		y = mission name, string (text)

	Inside the blank text file you made, use the template provided or fill in the following format:

	Mission Title
	Starting Time (leave it blank if you don't want to set time, valid number is 00:00 to 23:59)
	Coordinates in X, Y, and Z for an entry point
	Coordinates in X, Y, and Z where a terrorist spawn
	hostages
	Coordinates in X, Y, and Z where a hostage spawn

	Separate each format with a new line. You can add more terrorists and hostages by creating a new line and assign its coordinates.
	Don't forget to conclude with a new line underneath the last hostage spawn coordinates before saving.
	If you can't understand it by explanation, in the NooseMod folder contains 8 text files; open one of them to see how it forms.

18. DYOM Builder: ATM, to create a new mission, you must record the coordinates while playing GTA IV, and write it in a new text file manually. The tools you need are simple:
		- sjaak327's Native Trainer
		- any screen-capturing program such as Fraps (free version allows you saving image in BMP format)
		- of course the GTA IV or EFLC in latest version
		- Notepad

	Enable in Trainer "Display Coordinates" or similar like that and then find a good spot for entry point. Grab a shot.
	Place a coordinate where a terrorist will show. Grab a shot. Repeat until you have defined enough terrorists.
	Place a coordinate where a hostage will show. Grab a shot. Repeat until you have defined enough hostages.
	Exit the game and view the screenshots. Write the coordinates based on the template.

	In the new version I will include a script for building a NooseMod mission.


Requirements:
====
In order to test or recompile my source code, the requirements needed are:

1.	GTA IV version 1.0.7.0 or EFLC version 1.1.2.0
2.	LCPDFR version 1.1 (includes .NET ScriptHook) (the current version that's stable for 2 or more years)
3.	Visual Studio 2012 or later with .NET Framework 4.5
4.	Basic knowledge of C# programming language and GTA Scripting in C# language


How to Work:
====
Feel free to do whatever you like, as long as you follow the provided license.
However, the source code is used for learning purposes. It does not include other necessities.
You need to grab the necessities like NooseMod.ini, NooseMod folder, and many things from the release package.


Installation:
====
1.	Put NooseMod.net.dll, NooseMod.net.dll.config, NooseMod.ini and NooseMod folder into «yourgamedir»\LCPDFR\plugins directory.
2.	Put Audio folder to «yourgamedir»\LCPDFR directory.
3.	Put NAudio.dll to «yourgamedir» directory. If you have it from other mods (like JulioNIB's), disregard.
4.	Edit NooseMod.ini to your preferences.
5.	Start up the game and go on duty.


Uninstallation:
====
Simply remove NooseMod.net.dll, NooseMod.ini and NooseMod folder from «yourgamedir»\LCPDFR\plugins directory and you're done.
If you wish to keep the Stats you made so far, please store the Stats.mdb (from the NooseMod folder) to a safe location.


Gameplay:
====
For the starter, first of all, select a ped model (usually at the dressing room). Choose the SWAT/NOOSE model (regular/m_y_swat or helicopter SWAT/m_y_nhelipilot).
Get a partner (optional). If you travel alone, when a mission starts, a squad of NOOSE team will be following you on your tail.
Enter Chief of Police's office room and select a difficulty (if you haven't). Activated Hardcore mode in LCPDFR Settings will always have Hard difficulty (and that cannot be changed).
Then exit the station and choose a vehicle. Enforcer or any fast cruisers are recommended.
Patrol the Liberty City and wait for the callout from Dispatch.
When you receive a callout about "Possible terrorist activity", "Terrorist activity", "SOS", "Gang-related Violence", "Assault and Battery", or "A Criminal on a Gun Spree", respond. The mission then starts.
Go to mission location. You will see police officers stationing cars and roadblocks before you arrive. When you are at location, wait for the rest of the team to arrive. And be hurry before the terrors start picking off hostages.
Eliminate all suspects, but don't let hostages get hurt. If any of them flees, intercept until stopped, and it's all yours: kill or arrest. Arresting will gain more bonus than killing.
When mission ends you will get your reward depending on number of officers, suspects and hostages down (see Battle Results at Features section).

To start the next mission, kill some time by aiding the LCPD officers on the field before you can get a new callout about it again. Make sure you rearm before going into a gunfight.
Every mission will have a time limit until a hostage has been executed. Time your ETA precisely. Another time limit will be active if there is another hostage until all is executed and you wait for the mission to fail.


If you are selecting the model other than SWAT/NOOSE (M_Y_COP or standard policeman for example), the special callout "Terrorist Pursuit" will be assigned.
No stats will be recorded if you are playing with model other than SWAT/NOOSE, but you still be able to gain salary like the SWATs do (not just issuing parking ticket or issuing citation to people).


Issues:
====
"Night at Museum" mission only works in GTA IV because EFLC has 2 different entry points: TLAD at the back side, TBoGT from the ceiling.
It is possible to pass this mission in EFLC, but you have to do workarounds. Don't worry, all entry points are covered.


License:
====
This script mod follows the compliance of Naruto 607's Game Modding License and GNU Lesser General Public License (see GPL.txt and LGPL.txt).
Feel free to redistribute.

You are also free to modify or enhance this source code or the compiled library and release it as long as my name mentioned clearly in the Readme and the license must not be modified or tampered with.


Notes for _hax (the first person who built this NooseMod Beta script):
====
I kinda like this mod, but it's a pain in the ass telling that my latest game (GTA IV 1.0.7.0 and EFLC 1.1.2.0) do not support it anymore unless if I downgrade, which I have no idea where to start.
Also, it contains bugs based from the videos I saw. And the bad news is: your mod is never meant to be updated since it was released 8 years ago.
I am updating this on my own, with the help of ILSpy, thanks to Aquilon96's idea on GTAGaming Forums that already closed. AND by placing it on LCPDFR API (plugin) since it's a part of policing.
To note, I don't know how to PM you because GTAGaming has come to an end, and yet I haven't seen you talking for VERY LONG.

To _hax:
If you personally disagree with what I have done, please PM me stating so and I will remove it at your request, no arguments.


Contact:
====
Need to be in touch with my daily life and mod updates? Check out http://agungparamtapajournal.blogspot.com for more GTA and DCS modifications, as well as my life catalogue!
Need a hand? I'm available via Private Messages in:

	LCPDFR: Naruto607
	GTAInside: Naruto 607
	LockOnFiles: Naruto 607
	Facebook: Agung Paramtapa
	Google+: Agung Paramtapa


I hope you enjoy this plugin. It's not much, though.

Copyright © 2017 Naruto 607 Modifications
All Rights Reserved