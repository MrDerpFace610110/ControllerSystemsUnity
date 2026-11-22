<div align="center">

# ControllerSystemsUnity
*Utilization scripts of Unitys gamepad support*

![Unity](https://img.shields.io/badge/Unity-6-black?logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Mac%20%7C%20Linux-lightgrey)

</div>

## Current Features:
- **Multiple controller support**
- **Switching between keyboard and controller** for player 1
- **Rumble Effects** (One-shot and continuous)
- **Device-based glyphs**
- **Controller UI navigation**
- **Input rebinding during runtime**

## Planned Features:
- Gyro controls
- Touchpad
- Adaptive triggers
- Controller speaker input
- Controller mic input

---

## What's Included
The `.unitypackage` in the releases contains all the scripts included in this repo plus the additonal ones used for the 3 demos.

### Demo Scenes

**1. MainShowcase**
A showcase of multiple player support (up to 4), each controller navigating its attached player and handling its own rumble. Player 1 can switch between controller 1 and using the keyboard. If a controller is disconnected a pop-up will appear until it is reconnected. using the controler north and west buttons with trigger the rumble effects.

**2. ControlRebinds**
Showcases using a UI button the rebind the one shot rumble button. the button will be rebound in the InputAction for player one (while the scene is running) and the UI button glyphs will update accordingly.

**3. UINavigation**
Allows you to use a controller to switch between 3 buttons, pressing the south controller button will open a pop-up. (This is using the default UI InputActionAsset binding, if you do not have these it wont work)

## Credits:
The controller button glyphs are from **Button Prompt** by **VEXED** on itch:
[https://v3x3d.itch.io/button-prompt](https://v3x3d.itch.io/button-prompt)
