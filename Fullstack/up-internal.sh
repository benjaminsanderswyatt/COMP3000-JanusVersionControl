#!/bin/bash

# Ensure necessary directories exist
mkdir -p ./data/images
mkdir -p ./data/files
mkdir -p ./data/trees

docker-compose -f docker-compose-internal.yml up -d