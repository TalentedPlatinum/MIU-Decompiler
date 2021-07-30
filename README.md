# MIU-DECOMPILER
Programmer & Initial Tester: TalentedPlatinum\
Tester: VilleOlof
Tester: Anson

## Introduction
This is a Decompiler for Marble It Up!\
With this tool you can choose any .level file and decompile it back into a unity scene\
(Note to follow the Decompiler Workshop Rules pinned in #level-building if you’re planning on uploading levels using this tool)\
This tool was created over the period of a week and later designed for public release.

## How To Install
(Github screenshots of download)

Download the MIUDecompiler.zip file from the Github\
Extract the file

Now go into your Unity Project\
Click on Assets\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762521840210010/unknown.png)\
And choose the Import Package option and then Custom Package.\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762530904100884/unknown.png)\
Go and find the .package file you extracted from the .zip and select that.\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762538554519612/unknown.png)\
Here you can just click Import, don’t mess with anything else.\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762544682385428/unknown.png)\
It should now be installed and a new folder should have appeared in your Project’s Asset folder.

## User Guide

Now when you have it installed properly, you can go ahead and click on the Window button\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762553444290600/unknown.png)\
And then choose the MIU Decompiler window.\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762560276815882/unknown.png)\
You can drag around this window and place it wherever you want.\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762566941573160/unknown.png)\
To Decompile a level you just wanna press the button and select a .level file. \
Note that all MIU Classic Official Levels can be found in

>“C:\Program Files (x86)\Steam\steamapps\common\Marble It Up!\Marble It Up_Data\StreamingAssets\Levels”\

Or whatever location you chose for the game\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762574118002738/unknown.png)\
Just press Open and the decompiler will do its job and open up the level for you in Unity\
![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762581814575124/unknown.png)

When it Decompiling CloudClusterSky1,2,3 it will just not properly decompile them, therefore there is a button to do this. As far as we have tested it only affects certain official levels, Such as Bumper Invasion or Archiarchy.

![Alt text](https://cdn.discordapp.com/attachments/365772775832420353/870762587892092938/unknown.png)

## Some Important Information to note:
Note that this Decompiler was developed and tested in Unity Version 2019.4.24f1. Anything else might have issues or bugs that are not intended. For the best and most flawless experience, use any Unity Version of 2019.

All MTAs are not visible in Unity unless you respawn them (Tho they will appear like normal once Exported through the MIU level kit)

Signs and Prefabs will appear as checkerboarded cubes. These are pure visuals in Unity and have no effect in-game.\
Official levels have no Author/Medal times in their .level files so they will be blank.

Mayhem levels are not supported and cannot be Decompiled, once Mayhem releases on Steam the Decompiler will update and fully support them.

If you wanna see particles in Unity you just gotta click on them in the hierarchy but note that they will always appear as white squares 

If you find any bugs/errors or issues with the Decompiler you can send an issue via Github with the proper information (Such as the error if there is one, what scenario the bug/error happens in and how you made it happen)

If you wanna replace a prefab with your own prefab (It needs an IgnoreObject component) instead of the checkerboard cube, you just need to place the prefab file inside your MIU/Internal/Resources and then Decompile the level again

## Current TODO List

> MIU Search for materials in two searches, prioritizing MIU folder

> Bringer Of Lightmaps Update Lightmaps Inspector Option

> Advanced settings config file
>   - DTX1/5 Settings included

As a wise man once said:

>// Bail if we are empty and have no children.

LevelSerializer.cs Line 367
