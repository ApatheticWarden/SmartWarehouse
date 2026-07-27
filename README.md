## 🚀 A Personal Note from the Founder
Hey there! I’m one of the original co-founders of this project (and yes, also a student :P).

This initiative was initially designed for a cross-disciplinary team of 4-6 people. However, due to various scheduling uncertainties and life getting in the way, the project has been somewhat on hold recently. Despite the setbacks, I refuse to let this idea gather dust.

For the time being, I am stepping up to push the development forward on my own. My ultimate goal is to keep the momentum going, build out a solid foundation, and eventually merge this work back into the main repository. I want to see this through and make KODA proud of what we can achieve!

We might be flying solo for now, but the mission remains the exact same :D

## Overview
This repository contains the source code and documentation for our university science club (Koło Naukowe) project. 
We are building a budget-friendly, decentralized "goods-to-person" logistics ecosystem tailored for Small and Medium Enterprises (SMEs). 

This is not a multi-million-dollar industrial solution like Amazon Kiva. It is a scalable, low-cost Proof of Concept (PoC) designed to automate basic warehouse tasks and integrate with business management software (CRM/ECR) without requiring expensive infrastructure.

## Current Status
**Phase: MVP / Proof of Concept**
The project is in its early stages. We are currently developing the core communication layer between the central server and the first hardware prototype. The architecture is subject to change.

## Architecture & Tech Stack
The system is divided into three main logical layers:

* **Backend & Management (C# / .NET):** The central brain of the ecosystem. It manages the fleet of robots, distributes logistical tasks, and provides a UI for real-time monitoring. Designed for cross-platform deployment (Web, PC).
* **Algorithms (C++):** Core logic for pathfinding, task optimization, and navigation.
* **Embedded / Hardware (C++ / ESP32):** The physical Autonomous Mobile Robot (AMR). It handles asynchronous Wi-Fi communication with the server, parses commands, and controls the physical actuators and sensors.

## Hardware Prototype (Current Iteration)
To validate the software architecture and network latency, our first PoC robot is built using accessible components:
* **Microcontroller:** ESP32 
* **Motor Driver:** L298N (Dual H-Bridge)
* **Actuators:** Basic DC gear motors (Scheduled for an upgrade to metal-gear motors with encoders for precise odometry in future iterations).

## Repository Structure
- **docs/** - Project documentation and official PCz forms
- **firmware/** - Embedded code for the AMR robots (C++)
- **algorithms/** - Navigation and pathfinding logic (C++)
- **server/** - Backend ecosystem and User Interface (C# / .NET)
- **shared/** - Common data models and API contracts