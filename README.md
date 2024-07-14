## What's this script?
### This scripts records the changes within a folder and apply those changes in a target folder. The changes are logged in the console and saved in a logs.txt file which path has to be also defined. If file was already created, it won't create a new one. The interval marks how frequently the changes should be applied. One library was used to make the CLI easier: System.CommandLine "2.0.0-beta4.22272.1"

## How to run the script
### ./TestTask --source /home/antonio/Documents/src/ --replica /home/antonio/Documents/replica/ --log /home/antonio/Documents/ --interval 20

## Note
### This script was developed and tested in a linux system