#!/usr/bin/python
import sys
import numpy as np
import pandas as pd


def process_title(title, text):
    """
    Function to apply to dataframe to strip out texts from
    the title
    :param title: Title column in the dataframe
    :param text: Text column in the dataframe
    :return: New title
    """
    # If the title starts with the text, we want to remove the
    # text from the title in some conditions
    if title.startswith(text):
        # If the entire title is just the text, keep the text.
        if title == text:
            return title
        # If the title is just the text followed by part 1,
        # part 2, etc just keep the title
        elif (title[:len(text)] == text and
              title[len(text):len(text) + 6] == ", Part"):
            return title
        # Otherwise, take out text from the title by taking
        # out the first few characters from the title, the
        # number being equivalent to the length of the text
        else:
            new_title = title[len(text):]
            # Strip leading characters that are unneeded
            new_title = new_title.lstrip('-:, ab')
            return new_title
    else:
        return title

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

    # Edit the title column to take out the reference
    df["Title"] = np.vectorize(process_title)(df["Title"], df["Text"])

    # convert to csv
    df.to_csv("ExtractSermons.csv", index=False)

if __name__ == '__main__':
    main()