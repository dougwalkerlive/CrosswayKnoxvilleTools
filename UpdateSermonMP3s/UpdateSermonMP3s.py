#!/usr/bin/python
import os
import sys
import csv

def getMP3(path, URL):
  filename = URL #Need to strip the path
  print 'Downloading "' + filename + '"...'
  # Download command
  # Get file handle

# Get the filename from the commandline argument
if len(sys.argv) < 1:
  print 'Need to input a .csv filename as an argument.'
else:
  csv_filenames = sys.argv[1:]

# Create the download directory
download_dir = './mp3_downloads'
if not os.path.exists(download_dir):
  print 'Creating "' + download_dir + '"...'
  os.makedirs(download_dir)

# Loop over all csv files (probably unnecessary, but trivial to do)
for name in csv_filenames:
  print '\n\n\nProcessing file "' + name + '"...'
  file = open(name)
  # for line in csv.reader(file):
  #   print 'reader:', line

  # Process the header, assumed to contain the following fields:
  #   SermonID, Title, Speaker, Date, Text, Mp3Url, Description, SeriesName, SermonPageUrl
  fields = file.readline().split(',')
  print fields

  # Process the data for each line
  for line in csv.reader(file):
    # Convert the line to a dictionary for easy access
    mp3_dict = dict(zip(fields, line))

    # Download the file and get a handle to it
    getMP3(download_dir, mp3_dict['Mp3Url'])

    # Set the mp3 tags
