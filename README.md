# ANNs in VR 


# "Towards Explainable AI by Visualising Models of Artificial Neural Networks in Virtual Reality"

## Introduction

Welcome to my bachelor thesis project repository! This project includes a visualization prototype, where I explore various neural network architectures in virtual reality. This project aims to bring a deeper understanding of AI by leveraging the immersive capabilities of VR.

## Prerequisites

To get started with this project, you'll need:

- Unity (Download from [Unity Download](https://unity.com/download))
- Editor version: 2021.3.30f1 with Android Build Support
- VR Hardware: Primarily developed with the Quest 2, but compatible with Oculus Rift or Quest Pro etc.
- TensorFlow models: Available from this Flask server [Flask Server](https://git.opendfki.de/cowo01/thesis_repos) (follow the instructions provided there)

## Installation

Follow these steps to install and set up the application:

1. Clone the repository: `git clone https://github.com/LeonieGr/ANNs_in_VR.git`
2. Open the project in Unity (version 2021.3.30f1).
3. Start the server in your terminal.
4. Update the IP address for the Unity web request in the `GetApiData` script:
    - Replace the URL with your server’s IP address: 'http://<your ip address>/sequential/layer_info` (eg. `http://172.22.28.210:4999/sequential/layer_info`)

## Usage

To run the application:

1. Build the scene on Mac to run the application on the VR headset, or use Oculus Link.
2. The application is contained within the `mainVR Scene`.
3. In-VR instructions are provided for usage and navigation.

## Controls and Navigation

The application includes a comprehensive guide on using the handheld controllers. This guide is accessible within the VR environment.

## Features

The application boasts several key features:

- **Language Selection**: Choose between English and German.
- **Interactive Elements**: Initial zone with usage instructions, project introduction, and model selection for visualization.
- **Educational Content**: Learn about neural networks, layer functions, and activation functions in a dedicated area.
- **Model Visualization**: Central area for immersive model visualization and interaction.

## Contributing

Contributions to this project are welcome! If you're interested in enhancing the application or suggesting improvements...
