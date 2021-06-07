#!/usr/bin/env python

import glob
import re

def processLine(line):
    match = re.search("^\s+\-\s+.*", line)

    if match is None:
        return line

    return re.sub("(^\s*:)|(:\s*$)", "", match.string)


files = glob.glob('**/*.asset', recursive=True)

for filename in files:
    with open(filename) as f:
        lines = f.readlines()
        conversation_found = False
        for line in lines:
            if "conversation:" in line:
                conversation_found = True
                continue
            
            if not conversation_found:
                continue

            matches = re.findall("(^\s+\-\s+\)[:\s]*(.*)[:\s]*", line)

            if len(matches) == 0:
                continue

            print(matches)

            match = matches[0]
            processed_line = match[0] + match[2]
            print(processed_line)
            
    


            
