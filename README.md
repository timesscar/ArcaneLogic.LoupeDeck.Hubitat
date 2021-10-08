# ArcaneLogic.LoupeDeck.Hubitat

This is a basic [Loupedeck](https://loupedeck.com/us/) plugin that enables toggling devices connected to a Hubitat home automation node.

It has only been tested on the "Loupedeck Live" device.  

Disclaimer:  I am not associated in any way with the Loupedeck company. This plugin is implemented using the Loupedeck SDK, and is offered without warranty.  They maintain any trademark and copyrights over the SDK software.

[![Build Status](https://arcanelogic.visualstudio.com/ArcaneLogic.LoupeDeck.Hubitat/_apis/build/status/ArcaneLogic.LoupeDeck.Hubitat?branchName=main)](https://arcanelogic.visualstudio.com/ArcaneLogic.LoupeDeck.Hubitat/_build/latest?definitionId=16&branchName=main)


# Installation  
Copy the contents of the zip folder to the %LocalAppData%\Loupedeck\Plugins directory and restart the Loupedeck software.

# Usage
Set the API key and Hubitat maker URL in your app.config.  Then, assign the default device plugins to a button of your choice. 

# Customization
To customize images used for the actions, place your images in the "Images" directory, and update the AudioDevicePlugin.dll.config with your devices and image names in the "CustomImages" section.  To update the mute icon, you only need to update the MuteIcon and UnmuteIcon properties.

```  
  <hubitatPluginSettings HubitatApiUrl="Your local Hubitat maker URL" DpapiEncryptedApiKey="Your key encrypted with DPAPI">
    <CustomImages>
      <customImage DeviceName="Office Lamp" ImageName="lamp.png" />
      <customImage DeviceName="Upstairs Lamp 2" ImageName="invalid.png" />
    </CustomImages>
  </hubitatPluginSettings>
```
