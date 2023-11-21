

# Lethal Company Project Cleaner
**This cleans up Unity projects exported with AssetRipper in order to fix errors and make them usable for asset creation.**

- This program is absolute spaghetti and i did not intend for anyone else to use it but it may help someone?
- This was written for lethal company modding
- Only tested on projects exported with AssetRipper 0.3.4.0

## Installation

1. Download the latest release from [Releases](https://github.com/EvaisaDev/LethalCompanyProjectCleaner/releases)
2. Extract in any location

## Getting Ready
*You will need go through both sections listed here.*

#### **Exporting Asset Ripper Project**
1. Install [AssetRipper](https://github.com/AssetRipper/AssetRipper/releases)
2. Run AssetRipper and select the following settings:
	- Script Export Format: Hybrid
	- Script Content Level: Level 1
3. Click File -> Open Folder -> Select the "Lethal Company_Data" folder in your Game Folder.
4. After it is done loading game content, click Export -> Export All Files, and select a location.

#### **Creating Unity Project**
1. Install [Unity Hub](https://unity.com/download)
2. Open Unity Hub, and click on the "Installs" tab
	- If 2022.3.9f1 is already installed, skip to step 7
3. Click the Install Editor button
4. Go to the Archive tab, and click the "download archive" link
5. Find Unity 2022.3.9 and click the Unity Hub button next to it.
6. Go through the installation process.
7. Switch to the "Projects" tab
8. Click New Project, and select "3D (HDRP)"
	- You can name your project anything and select any location.
	- Then click Create Project
9. After unity finishes loading, continue to the next section.

##  Using the Project Cleaner

1. Run LethalCompanyProjectCleaner.exe
2. It will ask you to enter exported project folder
	- This is the AssetRipper exported project, it will be `Path/To/ExportFolder/Lethal Company/ExportedProject`
3. After you press enter it will ask for your new Unity project.
	- This is the path of the Unity HDRP project you created earlier.
4. Once you press enter it will process and move the game scripts/assemblies to your new unity project, and set up required packages, along with layers/tags.
5. You can now go back to your created unity project and use game components and such to make custom assets.
