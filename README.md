# vigo

A toolset for simple cross-platform rule-based file deployment.

[![.NET 8.x Build & Test](https://github.com/josenbo/vigo/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/josenbo/vigo/actions/workflows/dotnet.yml)

# Use-Case: Checking and Bundling File Collections for Deployment

The utility might serve your needs, if you intend to introduce automatic
deployments in an application environment consisting of shell scripts, 
cronjobs, SQL scripts, configurations and other files. 

The complete set of files is delivered in an archive including separate 
folders for each target environment. Each environment can have common as
well as distict files. Folder-specific rules identify the needed files, 
govern the renaming of files, specify the target environments and describe 
the transformations and checks to apply.

In addition to automatic deployments, the utility can be used in commit
checks to detect encoding issues close to the development activity.

An additional benefit might be, that uploading the archive to your 
artifact repository complements the version tag and helps to document
what effectively gets deployed.

