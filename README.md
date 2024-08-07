# PublishersFork
Unity3D core API fixes - NOT released by Unity, discovered/invented by AssetStore publishers

## Wat?
A large bundle of standalone files, each of which fixes or works around one or more bugs in Unity3D. Download whichever ones you need.

# Install instructions

## Option1: grab the files you want

...and drop them into your project. (almost) All files are standalone, and don't require any extra files.

## Option2: edit, customize, and submit pull-requests

Two optional .csproj files are included which let you open the files in your IDE and edit them there. This is a great way to double-check if there are any dependencies before adding a file to your project.

### Enabling the .csproj / .sln

**Critical**: For the .sln to work you must create a symlink after cloning, and it must be named "UnityEditorFolderSymlink". Without this, the IDE won't be able to find the Unity DLL's it needs to compile code.

#### Windows: 
1. Open a command prompt
2. Navigate to the folder that you cloned this project into
3. Create a 'hard junction' using this command line:

> mklink /D UnityEditorFolderSymlink "C:\path\to\your\Unity\install\Editor"

#### Mac / OSX:
1. Open a shell window
2. Navigate to the folder that you cloned this project into
3. Create a 'symbolic link' using this command line:

> ln -s /path/to/your/Unity/install/Editor UnityEditorFolderSymlink


# Background
As of Spring 2022, Unity is currently running 5-6 months behind on responding to bug reports and starting bugfixes; after they've started on a bug, it takes between 1 and 4 months for them to fix it (average: approximately 2 months). Once it's fixed, it takes them between 2 weeks and 4 months for them to publish the fix in the live versions of Unity.

After that ... it still takes anywhere from 3-24 months for most users of Unity to upgrade to a version that contains the fix.

Net result: When an AssetStore publisher finds a bug in Unity, isolates it, creates a reproducible test-case, and senda it off to Unity ... they still have to wait approximately 1 to 3 *years* before the fix will be live for all their end-users. Until then ... they need to maintain a workaround in their own codebase.

Over the last 15 years I've built up a large collection of automated fixes for Unity3D bugs that Unity has either been "slow", "very slow" to deal with ... or "won't" fix. I'm cleaning them up and sticking them here for others to borrow (and/or adapt) as they see fit.

# Usage instructions
From trial and error I've found the best way to handle this is "1 file per bugfix or workaround, with a verbose name that describes the bug or feature". This has made it extremely easy to add a fix to an existing/new project -- and easy to delete it from the project once we stop supporting the old versions of Unity that needed the fix.

So ... find a bug-fix/feature-improvement you like, and download that one file. Nearly always it will be self contained. Occasionally some will depend on others - I try to make this as rare as possible, but there are some low-level features missing from Unity that are so important they end up being used in a lot of places.
