#!/usr/bin/python
import sys
import json
import pandas as pd

# Get the filename from the commandline argument
if len(sys.argv) < 2:
    print('Need to input a .json filename as an argument')
    exit()
else:
    filename = sys.argv[1]

# load the data from the json file
sermons_df= pd.read_json(filename)

# convert to csv
sermons_df.to_csv("ExtractSermons.csv", index=False)
