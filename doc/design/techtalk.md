# TechTalk

## Random Notes

Motivation: Provide f

## Narrative

Provide the files for deployment

Why not just clone the required commit?
- Some files are not deployed (e.g. documentation)
- Some files are different in different environments (e.g. crontab)
- Ensure that files have proper encoding, newline and permissions

Liked the idea doing all the required transformations and bundling 
all files for a release in an archive. This is the starting point 
for legacy deployments with manual steps as well as for automatic
deployments on up-to-date machines

File name may include the release name. Produce root level files
with information about the release context. Should be in a format
which supports scripted evaluation and manual inspection.

Sample file name: ines_hub_release_2024_002_003.tar.gz

```text
ines-hub                      <== Name of the source repository
+-- production                <== Name of the target environment
|   +-- scripts
|   |   +-- ines
|   |   |   +-- local
|   |   |   +-- crontab
|   |   +-- stammaus
|   |   |   +-- local
|   |   |   +-- crontab
|   |   +-- cupines
|   |       +-- local
|   |       +-- crontab
|   +-- db
|       +-- build
|           +-- DANET
|           +-- NE_USER
|           +-- (others)
+-- non-production
    +-- scripts
    |   +-- ines
    |   |   +-- local
    |   |   +-- crontab
    |   +-- stammaus
    |   |   +-- local
    |   |   +-- crontab
    |   +-- cupines
    |       +-- local
    |       +-- crontab
    +-- db
        +-- build
            +-- DANET
            +-- NE_USER
            +-- (others)
```






## Use Cases



## Actions

### Commit Check

Make sure, that files have proper encode

1. Read with source encoding and write to target encoding
2. Rewind target and write to twin file with source encoding
3. Check if original and twin file are identical.
(Make sure to honor the UTF byte ordering flag)

### Deployment Bundles


## Todo

Introduce UTF-8 aliases (with, without, hoever) BOM
