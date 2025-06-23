VERY IMPORTANT: The file system has to be ext4. Some dependencies have filenames that differ only in capitalization. Trying to do this using WSL on an NTFS mounted directory (/mnt/<drive letter>) will fail. Do it somewhere in your $HOME directory.

The general guide is:

https://bitbucket.org/chromiumembedded/cef/wiki/MasterBuildQuickStart.md

https://bitbucket.org/chromiumembedded/cef/wiki/AutomatedBuildSetup.md

Let's assume, from now on, that you're working on `~/cef`

Install the build dependencies. I had to install some more after this, but this should take care of most of them. Also, because I'm using a more modern version of Ubuntu, I had to manually download and install some .deb packages that are deprecated and couldn't be installed via `apt`. I've added here the ons I had to install manually, but it may not be required with this branch

`cd ~/cef`

`sudo apt-get install curl file lsb-release procps python3 python3-pip`

`curl 'https://chromium.googlesource.com/chromium/src/+/main/build/install-build-deps.py?format=TEXT' | base64 -d > install-build-deps.py`

`sudo python3 ./install-build-deps.py --no-arm --no-chromeos-fonts --no-nacl`

`python3 -m pip install dataclasses importlib_metadata`

EXTRA STEP: Get an old version of the depot tools and put it in your path. This gets rid of some errors related to vpython later. Perhaps it's not needed with newer cef releases, but it won't hurt.

`cd ~/cef`

`git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git`

`mv depot_tools depot_tools_old`

`export PATH=~/cef/depot_tools_old:$PATH`

`cd depot_tools_old`

`git checkout -b chrome/3987 origin/chrome/3987`

Get the depot tools:

`cd ~/cef`

`git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git`

`export PATH=~/cef/depot_tools:$PATH`

Get the automate-git.py script:

`cd ~/cef`

`mkdir automate`

`cd automate`

`wget https://bitbucket.org/chromiumembedded/cef/raw/master/tools/automate/automate-git.py`

Create your chromium_git directory

`cd ~/cef`

`mkdir chromium_git`

Create your update script in `~/cef/chromium_git/update.sh` with the following content:

`#!/bin/bash`

`python3 ../automate/automate-git.py --branch=6099 --checkout=e6b45b0 --download-dir=. --depot-tools-dir=../depot_tools --no-distrib --no-build --no-depot-tools-update --force-update --force-patch-update`

Make it executable

`cd ~/cef/chromium_git/`

`chmod +x update.sh`

Configure your environment variables:

`export CEF_INSTALL_SYSROOT=arm64`

`export GN_DEFINES="is_official_build=true use_sysroot=true symbol_level=1 is_cfi=false use_thin_lto=false chrome_pgo_phase=0 enable_nacl=false use_x11=false use_ozone=true ozone_auto_platforms=false ozone_platform=headless ozone_platform_headless=true ozone_platform_gbm=false ozone_platform_wayland=false ozone_platform_x11=false"`

`export CEF_ARCHIVE_FORMAT=tar.bz2`

Run the update script:

`cd ~/cef/chromium_git`

`./update.sh`

If the script fails running runhooks, create another script in `~/cef/chromium_git/update2.sh` with this content:

`#!/bin/bash`

`python3 ../automate/automate-git.py --branch=6099 --checkout=e6b45b0 --download-dir=. --depot-tools-dir=../depot_tools --no-distrib --no-build`

Make executable and run the update2 script:

`cd ~/cef/chromium_git`

`chmod +x update2.sh`

`./update2.sh`

Then manually run the hooks (I believe this is the right directory, the log says where it's been run from).

`cd ~/cef/chromium_git/chromium/src`

`gclient runhooks`

Create your build script at `~/cef/chromium_git/build.sh` with these contents:

`#!/bin/bash`

`python3 ../automate/automate-git.py --branch=6099 --checkout=e6b45b0 --download-dir=. --depot-tools-dir=../depot_tools --minimal-distrib --client-distrib --build-target=cefsimple --arm64-build --no-update --verbose-build --force-build`

Patch cef:

`cd ~/cef/chromium_git/chromium/src/cef`

`git apply 120.1.8+ge6b45b0+chromium-120.0.6099.109.patch`

Make executable and run the build script:

`cd ~/chromium_git`
`chmod +x build.sh`
`./build.sh`

Fix any other build issues, may it be dependencies or compilation errors.