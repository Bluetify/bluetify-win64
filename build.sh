#!/bin/bash

dotnet publish -c Release -r win-x64 --self-contained true

echo "Build completed!"