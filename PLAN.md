# PLAN.md

# Project: OpenLogi (Working Title)

> A modern, lightweight, open-source replacement for Logitech G Hub focused on mice.

---

# 1. Vision

OpenLogi exists because Logitech G Hub has become increasingly frustrating for many users.

Common complaints include:

- Large memory usage
- Slow startup
- Bugs
- Broken updates
- Poor reliability
- Forced account features
- Poor profile management
- Unnecessary complexity
- Lack of transparency

Our goal is **not** to clone G Hub.

Our goal is to build the application we wish Logitech had built.

The application should feel like:

- VS Code
- Steam
- Discord
- PowerToys

Fast.

Stable.

Predictable.

Native.

No nonsense.

---

# 2. Mission Statement

Provide the best Logitech mouse software available.

Not the biggest.

Not the prettiest.

The most reliable.

The application should disappear into the background.

Users should install it once and never think about it again.

---

# 3. Philosophy

Core values:

- Open Source
- Zero telemetry
- Extremely lightweight
- Native performance
- Offline-first
- Transparent
- Predictable
- User owns their data
- User owns their hardware

Never require:

- Logitech account
- Internet
- Cloud sync
- Analytics
- Ads
- Subscription

---

# 4. Primary Goals

Replace Logitech G Hub for:

- DPI
- Polling rate
- Button remapping
- Macros
- Profiles
- Per-application profiles
- Automatic profile switching
- Battery status
- Device information

while consuming significantly less CPU and memory.

---

# 5. Non Goals

Version 1 intentionally does NOT support:

- Keyboards
- Headsets
- Speakers
- Webcams
- Racing wheels
- StreamCam
- Blue microphones
- Firmware flashing
- RGB synchronization
- Discord integrations
- Game integrations
- Cloud syncing

Focus.

One thing.

Do it exceptionally well.

---

# 6. Target Platforms

Version 1

Windows 11

Windows 10 (best effort)

Future

Linux

macOS

---

# 7. Supported Device Tiers

---

## Tier 1 (Full Support)

Every feature should work.

Every bug should be investigated.

Every release tested.

### G502 Family

- G502 Proteus Core
- G502 Proteus Spectrum
- G502 HERO
- G502 LIGHTSPEED
- G502 X
- G502 X LIGHTSPEED
- G502 X PLUS

---

### Superlight Family

- G Pro X Superlight
- G Pro X Superlight 2

---

## Tier 2

Expected to work.

Capability based.

Includes:

- G903
- G703
- G703 HERO
- G703 LIGHTSPEED
- G703 SE
- G305
- G303
- G303 Shroud
- G403
- G402
- G400s
- G600
- MX Master (where feasible)
- MX Anywhere

---

## Tier 3

Generic Logitech HID++

Whatever features the mouse exposes.

No guarantees.

---

# 8. Generic Compatibility Philosophy

Never hardcode functionality unless absolutely required.

Instead:

Mouse connects

↓

Discover capabilities

↓

Build runtime capability graph

↓

Enable supported UI

Unknown devices should degrade gracefully.

---

# 9. Major Features

## Device Detection

Automatically detect

- USB
- Wireless
- Lightspeed
- Receiver changes
- Sleep
- Wake
- Reconnect

---

## DPI

Support

Current DPI

Multiple DPI stages

Rename stages

Delete stages

Add stages

Sensitivity slider

Live preview

Instant apply

---

## Polling Rate

Support

125 Hz

250 Hz

500 Hz

1000 Hz

Higher polling where hardware supports it.

---

## Profiles

Local only.

Stored on machine.

Features

Unlimited profiles

Export

Import

Duplicate

Rename

Default profile

Auto switching

---

## Per Application Profiles

Example

chrome.exe

↓

Productivity profile

---

Cyberpunk2077.exe

↓

Gaming profile

---

VisualStudio.exe

↓

Developer profile

---

Automatic.

Instant.

No user interaction.

---

## Button Mapping

Every button should support

Mouse

Keyboard

Media

Application launch

Volume

Windows shortcuts

Clipboard

Browser navigation

Macros

Future plugins

---

## Macro Engine

Support

Key press

Key release

Mouse click

Mouse movement

Delay

Repeat

Conditional repeat

Media

Clipboard

Launch application

Open URL

Text typing

Future scripting support

---

# 10. Macro Philosophy

Macros should be simple.

Visual.

Powerful.

Never require scripting.

95% of users should never need code.

---

# 11. Architecture

UI

↓

Background Agent

↓

Core Library

↓

Device Layer

↓

HID Layer

↓

Windows HID API

---

# 12. Components

Core

Responsible for

Profiles

Settings

Capability graph

Configuration

Logging

Events

---

Background Agent

Runs silently.

Responsible for

Device monitoring

Profile switching

Macro execution

Foreground application detection

---

UI

Displays

Connected devices

Configuration

Macros

Diagnostics

Logs

Updates

---

Device Layer

Abstracts

Every mouse.

No device-specific code leaks upward.

---

HID Layer

Raw communication.

No business logic.

---

# 13. Device Abstraction

Every device implements

IDevice

Functions include

Initialize()

GetCapabilities()

ApplySettings()

ReadBattery()

ReadCurrentDPI()

SetPollingRate()

MapButtons()

---

Everything above the device layer becomes completely generic.

---

# 14. Capability System

Instead of

if (mouse == G502)

do

if (SupportsPollingRate)

Show polling UI.

This dramatically reduces future maintenance.

---

# 15. Local Storage

SQLite

Tables

Devices

Profiles

Applications

Macros

Macro Steps

Settings

Diagnostics

---

# 16. Logging

Every operation should be loggable.

Examples

Mouse connected

Profile loaded

Polling updated

Macro executed

Profile switched

Unknown HID packet

Errors

---

# 17. Diagnostics Mode

Users should be able to click

Export Diagnostics

Result

ZIP

Contains

Logs

Connected devices

Capabilities

Windows version

Application version

No personal information.

---

# 18. Unknown Device Workflow

Unknown mouse

↓

Read capabilities

↓

Store packet logs

↓

Offer export

↓

Community submits logs

↓

Future support improves

---

# 19. Testing Philosophy

Every supported mouse should have

Smoke tests

Reconnect tests

Sleep tests

Battery tests

Profile switching tests

Button mapping tests

Macro tests

Stress tests

---

# 20. Automated Testing

Mock HID devices

Capability tests

Regression tests

Database tests

Profile parser tests

Macro engine tests

---

# 21. Performance Goals

Cold launch

< 300 ms

Memory

< 80 MB

Background idle

< 15 MB preferred

CPU idle

0%

Profile switching

< 50 ms

Button latency

Imperceptible

---

# 22. Reliability Goals

Application should survive

USB unplug

Receiver reconnect

Sleep

Hibernate

Fast startup

Multiple users

Multiple mice

Unexpected HID packets

Unknown firmware

---

# 23. Security

No admin required if possible

Signed releases

No telemetry

No analytics

Offline first

No cloud

No internet dependency

---

# 24. Future Plugin System

Allow plugins to provide

Actions

Macros

Integrations

Custom devices

Without changing core.

---

# 25. UI Philosophy

Minimal.

Fast.

No animations unless useful.

Keyboard navigable.

Dark mode.

Light mode.

Accessibility.

---

# 26. Nice Future Features

Battery history

Usage statistics

Profile sharing

Community profile repository

Lua scripting

Rust plugins

Game detection improvements

Portable mode

Auto profile suggestions

---

# 27. Development Roadmap

## Phase 0

Research

- HID++
- Existing open source projects
- Windows HID
- Reverse engineering
- Device testing

---

## Phase 1

Infrastructure

Repository

CI

Logging

Core library

SQLite

Settings

---

## Phase 2

Device Discovery

USB

Lightspeed

Enumeration

Capability discovery

Diagnostics

---

## Phase 3

Core Mouse Features

DPI

Polling

Battery

Buttons

Device info

---

## Phase 4

Profiles

Profile manager

Application detection

Auto switching

Import

Export

---

## Phase 5

Macros

Macro editor

Execution engine

Validation

Performance optimization

---

## Phase 6

UI Polish

Accessibility

Animations

Icons

Diagnostics

Localization support

---

## Phase 7

Beta

Community testing

Bug fixing

Documentation

Performance tuning

---

## Phase 8

1.0 Release

Documentation

Website

Installer

Portable build

GitHub release

---

# 28. Risks

Different firmware revisions

Receiver differences

Undocumented HID++ behavior

Windows input quirks

Raw Input edge cases

Anti-cheat interactions

Unexpected Logitech firmware updates

Users expecting keyboard support

---

# 29. Success Metrics

Replace G Hub entirely for:

✓ G502 family

✓ Superlight family

Support most Logitech mice without crashing.

Consume a fraction of G Hub's resources.

Never require an account.

Remain open source.

Become the recommended community alternative.

---

# 30. Long-Term Vision (2.0+)

After establishing ourselves as the definitive Logitech mouse manager, expand carefully:

- Logitech keyboards
- Headsets
- Steering wheels
- Cross-platform support (Linux/macOS)
- Plugin ecosystem
- Community device database
- Shared profile marketplace
- Automated capability updates
- Rich diagnostics for unsupported devices

The project should always prioritize stability and quality over feature count. If adding a feature would compromise reliability or significantly increase maintenance burden, it should be deferred or omitted.

---

# Appendix A: Engineering Principles

1. Capability-driven, not model-driven.
2. Favor composition over inheritance.
3. Minimize global state.
4. Separate transport (HID) from business logic.
5. Treat every hardware interaction as fallible.
6. Prefer immutable configuration models.
7. Design every subsystem for unit testing.
8. Log enough to reproduce bugs without exposing user data.
9. Optimize for maintainability before micro-optimization.
10. Every release must leave the software simpler, more reliable, or more capable than the previous one.

---

# Appendix B: Definition of Done (v1.0)

A user with any supported G502 or G Pro X Superlight (1 or 2) should be able to:

- Uninstall Logitech G Hub.
- Install OpenLogi.
- Configure DPI and polling rate.
- Create and switch profiles automatically by application.
- Remap every supported button.
- Create and execute macros.
- View battery and device status where available.
- Export/import profiles.
- Use the software for months without crashes or unexpected behavior.

If those users never need to reinstall G Hub, version 1.0 has achieved its primary objective.
