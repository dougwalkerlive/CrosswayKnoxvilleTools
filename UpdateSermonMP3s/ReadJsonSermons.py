#!/usr/bin/python
import sys
import pandas as pd

def main():
    # Get the filename from the commandline argument
    if len(sys.argv) < 2:
        print('Need to input a .json filename as an argument')
        exit()
    else:
        filename = sys.argv[1]

    # load the data from the json file as pandas dataframe
    df = pd.read_json(filename)

    # Drop the descriptions column since it is always empty.
    df = df.drop("Description", axis=1)

    # Add column with the index number within each series
    df["SeriesIndex"] = df.groupby("SeriesName")["Date"].rank(method="first", ascending=True)
    df.SeriesIndex = df.SeriesIndex.astype(int)
    
    # convert to csv
    df.to_csv("ExtractSermons.csv", index=False)

if __name__ == '__main__':
    main()