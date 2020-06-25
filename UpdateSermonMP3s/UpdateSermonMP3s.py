#!/usr/bin/python
import os
import sys
import csv
import urllib.request
import urllib.parse
import eyed3

# Downloads an mp3, saves in a specified directory, and returns a file handle
def getMP3(path, URL):
  # Strip the path to get the file name
  filename = os.path.basename(URL)
  print('Downloading "' + filename + '" to "' + path + '"...')

  # Add escape characters to the file name
  filename_escaped = urllib.parse.quote(filename)
  # Get the escaped version of the URL
  url_escaped = URL[:len(URL) - len(filename)] + filename_escaped

  # Download command
  filename_fullpath = path + '/' + filename
  urllib.request.urlretrieve(url_escaped, filename_fullpath)

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
  file.tag.year = str(dictionary['Date'].split('-')[0])
  file.tag.comment = str(dictionary['Text'])
  file.tag.track_num = str(dictionary['SeriesIndex\n'])
  file.tag.genre = 'Sermon'
  file.tag.save()

def main():
  # Get the csv filename from the commandline argument
  if len(sys.argv) < 2:
    print('Need to input a .csv filename as an argument.')
    exit()
  else:
    csv_filenames = sys.argv[1:]

  # Create the download directory
  download_dir = './mp3_downloads'
  if not os.path.exists(download_dir):
    os.makedirs(download_dir)

  # Get list of all files currently in the download directory
  current_files_list = os.listdir(download_dir)

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
    #   SermonID, Title, Speaker, Date, Text, Mp3Url, Description, SeriesName, SermonPageUrl, SeriesIndex
    fields = file.readline().split(',')

    # Process the data for each line of the csv
    for line in csv.reader(file):
      # Skip blank lines - Only happens at the end of the file, so it doesn't really matter,
      # but the error at the end is still annoying
      if not line:
        continue

      # Convert the line to a dictionary for easy access
      mp3_dict = dict(zip(fields, line))

      # Check if file has already been downloaded
      filename = os.path.basename(mp3_dict['Mp3Url'])
      if filename not in current_files_list:
        # Download the file and get a handle to it if it is not
        # already in the directory
        mp3_file = getMP3(download_dir, mp3_dict['Mp3Url'])
      # If the file has already been downloaded, simply
      # get the handle
      else:
        print(filename, " has already been downloaded")
        mp3_file = eyed3.load(download_dir + '/' + filename)

      # Set the mp3 tags
      setMP3tags(mp3_file, mp3_dict)

      nSermons = nSermons + 1

  print(str(nSermons) + ' files downloaded and tagged...')
  print('Sermon tagging complete.')

if __name__ == '__main__':
    main()