#!/usr/bin/python
import os
import sys
import json

# Get the filename from the commandline argument
if len(sys.argv) < 1:
    print('Need to input a .json filename as an argument')
    exit()
else:
    filename = sys.argv[1]

# load the data from the json file
with open(filename) as file:
    sermons = json.load(file)

# print the title of each sermon
for s in sermons:
    print(s['Title'] + ' by ' + s['Speaker'])

