# Project: Laminar

## About

Project: Laminar is an open-source, cross-platform, plugin based automation framework, built to take your PC usage to the next level. From scheduling events throughout the day, to making complex macros to control your computer with your keyboard, and even turning your smart lights red whenever you get a work email, Laminar offers you total control.

This is the source code of Project: Laminar. We are currently under development and the documentation isn't complete, but if you want to make a plugin there are plenty of examples under src/Plugins to browse and learn from.

To get notified when we do a full release, join our Discord https://discord.gg/7CRfxG4T

If you want to help or have any questions, don't hesitate to contact us under projectlaminar@gmail.com.

## Building

Until the public release comes with a stable plugin framework, you will need to use one of:
- [Laminar.Build](build/Laminar.Build) to compile the plugin framework, update the internal plugin version, restore and build Project: Laminar
- [Laminar.Run](build/Laminar.Run) to call the builder, and then run a bootstrapped version of the main application. This is intended as a one button runner for active development of the plugin framework, where it will need to be rebuild constantly.

After running the builder from either project, it is possible to target [Laminar.Avalonia](src/Application/Laminar.Avalonia) directly for faster compile times, but this will assume a stable plugin framework.
