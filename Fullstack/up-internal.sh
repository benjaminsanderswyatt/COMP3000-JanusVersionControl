#!/bin/bash

# Ensure necessary directories exist
mkdir -p ./data/images
mkdir -p ./data/files
mkdir -p ./data/trees

# Copy 0.png into the images folder
cp ./frontend/public/0.png ./data/images/0.png

docker-compose -f docker-compose-internal.yml up -d