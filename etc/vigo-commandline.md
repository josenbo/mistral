# vigo Command Line and Configuration

Build a .tar.gz archive including all build targets

```bash
vigo build --repository-root <directory> --output-file <filepath>
```
export VIGO_REPOSITORY_ROOT
export VIGO_OUTPUT_FILE

Build a .tar.gz archive with the given build targets

```bash
vigo build --repository-root <directory> --output-file <filepath> --targets <listOfTargets>
vigo build -r                <directory> -o            <filepath> -t        <listOfTargets>
```
export VIGO_REPOSITORY_ROOT  
export VIGO_OUTPUT_FILE
export VIGO_TARGETS

Check the repository

```bash
vigo check --repository-root <directory>
vigo check -r                <directory>
```

Show how the configuration is applied to the folder content

```bash
vigo <file> <filenames>
vigo explain --configuration-file <filepath> --names <listOfFilenames>
vigo explain -c                   <filepath> -n      <listOfFilenames>
```

--help | -h
--version | -v


--repository-root

--temp <directory>
export VIGO_TEMP

--logfile <filepath>
export VIGO_LOG_FILE

--loglevel <enum>
export VIGO_LOG_LEVEL

