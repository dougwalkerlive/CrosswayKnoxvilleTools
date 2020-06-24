#!/usr/bin/python
import os
import sys
import csv
import urllib
import eyed3

# Downloads an mp3, saves in a specified directory, and returns a file handle
def getMP3(path, URL):
  # Strip the path
  filename = os.path.basename(URL)
  print('Downloading "' + filename + '" to "' + path + '"...')

  # Download command
  filename_fullpath = path + '/' + filename
  urllib.urlretrieve(URL, filename_fullpath)

  # Get file handle
  file = eyed3.load(filename_fullpath)
  return file

# Sets tags on mp3 file and saves it
def setMP3tags(file, dictionary):
  # Strip the path
  filename = os.path.basename(dictionary['Mp3Url'])
  print('Adding ID3 tag information to "' + filename + '"...')

  # Apparently this is a thing that's possible...
  if not file.tag:
    file.initTag()

  # Now set the data since we are guaranteed to have a tag
  file.tag.artist = str(dictionary['Speaker'])
  file.tag.album_artist = str(dictionary['Speaker'])
  file.tag.album = str(dictionary['SeriesName'])
  file.tag.title = str(dictionary['Title'])
  file.tag.year = str(dictionary['Date'].split(' ')[0])
  file.tag.comment = str(mp3_dict['Text'])
  file.tag.genre = str('Sermon')
  file.tag.save()

# Get the filename from the commandline argument
if len(sys.argv) < 1:
  print('Need to input a .csv filename as an argument.')
  exit()
else:
  csv_filenames = sys.argv[1:]

# Create the download directory
download_dir = './mp3_downloads'
if not os.path.exists(download_dir):
  os.makedirs(download_dir)

# Set the error level to "error" since there will be warnings for every file
#   - WARNING:eyed3.mp3.headers:Lame tag CRC check failed
#   - WARNING:eyed3.id3:Non standard genre name: Sermon
eyed3.log.setLevel("ERROR")

# Loop over all csv files (probably unnecessary, but trivial to do)
nSermons = 0
for name in csv_filenames:
  print('Processing file "' + name + '"...')
  file = open(name)

  # Process the header, assumed to contain the following fields:
  #   SermonID, Title, Speaker, Date, Text, Mp3Url, Description, SeriesName, SermonPageUrl
  fields = file.readline().split(',')

  # Process the data for each line
  for line in csv.reader(file):
    # Skip blank lines - Only happens at the end of the file, so it doesn't really matter,
    # but the error at the end is still annoying
    if not line:
      continue

    # Convert the line to a dictionary for easy access
    mp3_dict = dict(zip(fields, line))

    # Download the file and get a handle to it
    mp3_file = getMP3(download_dir, mp3_dict['Mp3Url'])

    # Set the mp3 tags
    setMP3tags(mp3_file, mp3_dict)

    #TODO: Need to go through and strip Scripture references out of the sermon titles
    #
    nSermons = nSermons + 1

# TODO: Would be kind of nifty to go through at the end and set tag.track_num
#   - Would just need to start at earliest date and count sermons in each series

print(str(nSermons) + ' files downloaded and tagged...')
print('Sermon tagging complete.')
