# [Download here!](https://github.com/knah/VRCLinuxAssistant/releases/latest)

VRChat Melon Assistant is a Linux mod installer for VRChat. It is a port of [VRCMelonAssistant](https://github.com/knah/VRCMelonAssistant) 
made with [Avalonia](https://github.com/Avalonia/Avalonia)

It uses mods published in [VRChat Modding Group Discord](https://discord.gg/rCqKSvR).  
It's a (very stripped down) port of a (very stipped down) fork of [Assistant's Mod Assistant](https://github.com/Assistant/ModAssistant), a mod manager for Beat Saber.  

**Modifying the VRChat client is not allowed by VRChat Terms of Service and can lead to your account being banned.** Mods available via this installer are manually checked to minimize the chance of that happening, but the risk is always there.  
VRChat Melon Assistant is not affiliated with and/or endorsed by VRChat Inc.

* [Linux Disclaimer](#Disclaimer)
* [Features](#Features)
* [Usage](#Usage)
* [TODO](#Todo)
* [Common Issues](#Common-Issues)

## Features

VRChat Linux Assistant boasts a rich feature set, some of which include:
* Installed mod detection
* Mod uninstallation
* Broken mod move-aside (temporarily uninstalls them until a fix is available)

## Usage
Download the newest build from the release section and run it. This application auto-updates when launched, there is no need to download a new release each time.  
Then, simply check off the mods that you wish to install or update and click the <kbd>Install or Update</kbd> button. Likewise, click the <kbd>Uninstall</kbd> button to remove any mods.

## Todo
Since this is a port to Avalonia, not everything will be ported out of the box.

Some Planned features are:
* Mod Info Page
* Support for BSMA Themes
* Headpats and Hugs

## Common Issues
**I hit install but I don't see anything in game!**
  Double check that you followed the [Usage](#usage) instructions correctly.  
  Make sure you're looking in the right place. Sometimes mod menus move as modding libraries/practices change.  
  Additionally, make sure the proper VRChat installation directory is selected in option tab.
  
**I don't see a certain mod in the mods list!**
  VRChat Melon Assistant uses mods from VRChat Modding Group and shows whatever is available for download. It's recommended to avoid non-VRCMG mods due to rampant spread of malware disguised as mods.
  
**I hit install but now my game won't launch, I can't click any buttons, I only see a black screen, etc**
  Please visit the [VRChat Modding Group](https://discord.gg/rCqKSvR) `#help-and-support` channel. Check the pinned messages or ask for help and see if you can work out things out.

## Disclaimer
There are no native ports of MelonLoader to Linux. Should this change in the future, I will update the assistant to differentiate between Linux and Windows builds of VRChat, to download the correct version of Melonloader.

PLEASE NOTE:
Most, if not all of the actual logic behind this was made by [Knah](https://github.com/knah).
I merely ported the UI to Avalonia, and fixed anything that broke. Since the architecture behind Avalonia is a bit more complex than just adding it as a dependency,
i couldn't directly fork his project.

## Credits
Lemon icon from Twitter Emoji
https://github.com/twitter/twemoji

Original Mod Assistant is made by Assistant and used under the terms of MIT License.  
https://github.com/Assistant/ModAssistant

Original Fork of the Assistant by Knah.
https://github.com/knah

semver by Max Hauser  
https://github.com/maxhauser/semver
