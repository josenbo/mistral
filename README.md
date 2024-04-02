# Simple Rule-Based File Deployment.

[![CI](https://github.com/josenbo/vigo/actions/workflows/build-and-test-net8.yml/badge.svg?branch=main)](https://github.com/josenbo/vigo/actions/workflows/build-and-test-net8.yml)

The vigo tool set is a minimalistic build tool with the aim of providing
repository files filtered by name patterns and transformed to the expected encoding and format as a single archive for deployment on the target
environment.

## Checking and Bundling File Collections for Deployment

The utility might serve your needs, if you intend to introduce automatic
deployments in an application environment consisting of shell scripts, 
cronjobs, SQL scripts, configurations and other files. 

The complete set of files is delivered in an archive, including separate 
folders for each target environment. Each environment can have common as
well, as distinct files. Folder-specific rules identify the needed files, 
govern the renaming of files, specify the target environments, and describe 
the transformations and checks to apply.

In addition to automatic deployments, the utility can be used as part of 
the commit checks to detect encoding issues close to the development 
activity.

An additional benefit might be, that uploading the archive to your 
artifact repository complements the version tag and helps to document
what effectively gets deployed.

## Defining the Rules - Folder-Specific Deployment Configuration

*This is just a brief introduction; see the [Vigo Wiki] for detailed 
information.*

For each folder having files that you want to deploy, create a file 
named deployment-rules.md or deployment-rules.vigo. Let's say, we 
have distinct mail gateway settings for each environment, and that 
they are defined in files named with an environment part like so:

- mail-gateway-test.json
- mail-gateway-production.json

Files in the repository are UTF-8 encoded, but the mail gateway 
settings need to be delivered to the target environment with 
ISO-8859-1 encoding and Unix-style text format. We would select 
the file for production and describe its transformation with the 
following rule:

```
DO DEPLOY TEXT FILE IF NAME EQUALS mail-gateway-production.json
    
    RENAME TO mail-gateway.json
    FILE MODE 644
    SOURCE ENCODING UTF-8
    TARGET ENCODING ISO-8859-1
    NEWLINE STYLE LINUX 
    ADD TRAILING NEWLINE true
    VALID CHARACTERS AsciiGerman
    BUILD TARGETS production
DONE
```

Later on, the vigo tool will walk the repository folder tree, read the configuration file and perform the file name matching, renaming, content transformation and copying to the build target folder in the compressed tar archive, which is called a deployment bundle.

## Building the Archive - Command Line and Environment Parameters

*This is just a brief introduction; see the [Vigo Wiki] for detailed 
information.*

The vigo tool is called with the path of the repository's top-level directory and the name of the deployment bundle file.

```bash
vigo <repository-dir> [-b <deployment-bundle-file>]
```

**&lt;repository-dir&gt;**

Define the top-level directory for walking the 
repositories directory tree and processing files.
This value is required and must point to an 
existing directory. The value may also be passed 
in the VIGO_REPOSITORY environment variable.

**[-b, --bundle &lt;deployment-bundle-file&gt;]**

Optional deployment bundle file. The file name must
have a .tar.gz suffix. If a relative or full path 
is given, it must point to an existing directory.
The value may also be passed in the VIGO_BUNDLE 
environment variable. Omitting this parameter is 
intended for commit checks, where the return code
alone is sufficient.


[Vigo Wiki]: https://github.com/josenbo/vigo/wiki
