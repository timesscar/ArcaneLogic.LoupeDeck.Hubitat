# ArcaneLogic.LoupeDeck.WindowsAudio

This is a basic [Loupedeck](https://loupedeck.com/us/) plugin that enables setting the current default Windows playback device, and the mute/volume state of the default Windows recording device.

It has only been tested on the "Loupedeck Live" device.  

Disclaimer:  I am not associated in any way with the Loupedeck company. This plugin is implemented using the Loupedeck SDK, and is offered without warranty.  They maintain any trademark and copyrights over the SDK software.

[![Build Status](https://arcanelogic.visualstudio.com/ArcaneLogic.LoupeDeck.WindowsAudio/_apis/build/status/ArcaneLogic.LoupeDeck.WindowsAudio?branchName=main)](https://arcanelogic.visualstudio.com/ArcaneLogic.LoupeDeck.WindowsAudio/_build/latest?definitionId=16&branchName=main)


# Installation  
Copy the contents of the zip folder to the %LocalAppData%\Loupedeck\Plugins directory and restart the Loupedeck software.

# Usage
Assign the default device plugins to a button of your choice. 

Assign the microphone mute click action and adjustment action to a dial of your choice.  The plugin currently only supports having both actions assigned to the same knob, however I intend on supporting just the mute/unmute action to a button in the future.

# Customization
To customize images used for the actions, place your images in the "Images" directory, and update the AudioDevicePlugin.dll.config with your devices and image names in the "CustomImages" section.  To update the mute icon, you only need to update the MuteIcon and UnmuteIcon properties.

```  
<windowsAudioPluginSettings VolumeChangeCooldown="2000" MuteIcon="mic-muted.png" UnmuteIcon="mic-unmuted.png">
  <CustomImages>
    <customImage DeviceName="Realtek Digital Output (Realtek(R) Audio)" ImageName="speaker.png" />
    <customImage DeviceName="Headphones (SMSL USB DAC)" ImageName="headphones.png" />
  </CustomImages>
</windowsAudioPluginSettings>
```
