# Scanning and Evaluating Tags in File Names

When staging files for deployment, we may want to make 
some files visible only to chosen environments and prevent 
deployment to others. A use case for this would be the 
cronjob file when there are monitoring and reporting 
jobs that will run only in the production environment.

The file name tagging library supports this requirement 
by allowing users to put tags in the file name, which 
are evaluated and then removed. Let us look at a simple 
example to clarify the library's purpose.

The file *cronjob-myuser* contains the crontab for the user 
*myuser*. Most of the jobs in the file need to be run in 
the development, test and production environments. There 
are some jobs that will only be executed in the production 
environment. So we need one *cronjob-myuser* file for 
production and another one for development and test. So we 
name the two files like this:

**cronjob-myuser<span style="background-color:yellow;color:blue">~DEPLOY~ONLY~production~~</span>**

and 

**cronjob-myuser<span style="background-color:yellow;color:blue">~DEPLOY~ONLY~development~test~~</span>**

When you ask the library to check a file name, it will 
look for delimiters, keywords and tags. If none are found, 
it will just return the original file name. If they are 
present and the syntax is valid, it will return the cleansed 
file name along with information on whether the file can be 
deployed in the current environment. When syntax rules are 
violated, it will throw an exception.



