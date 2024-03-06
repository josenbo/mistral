# Deployment configuration for the files in this folder

A detailed overview of the configuration syntax and the 
deployment process can be found [here on Confluence](https://confluence.dhl.com/x/7uPVGg).

## Setting default values for all the files in the folder

There are global default values which come into play, when there
is no folder-specific default or file-specific value defined in
this configuration file.

```vigo

vîgô - this is the marker used to recognize a folder configuration

CONFIGURE FOLDER

	# Expect only the universal ASCII 7-bit characters 
	# or [äöüÄÖÜß€] in the text file contents
    DEFAULT FOR VALID CHARACTERS AsciiGerman
    
	# Files will be deployed to the Prod and NonProd
	# build targets 
	DEFAULT BUILD TARGETS Prod, NonProd

DONE

```

## Defining rules for file handling

Rules are applied in the order of appearance in this configuration file.

## Check files with AsciiGerman characters

We are relying on the defaults, when checking files using the AsciiGerman 
characters. If you change the default, please revise these rules.

```vigo

DO CHECK TEXT FILE IF NAME EQUALS sample.bin
DONE

```

### Check the remaining files with Ascii characters

```vigo

DO CHECK ALL TEXT FILES
	
    TARGET ENCODING ASCII
    VALID CHARACTERS Ascii
DONE

```

### What happens with the other files?

All the files not matching any of the above rules will not 
be copied to the tar file
