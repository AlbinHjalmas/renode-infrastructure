#!/bin/bash

set -e
set -u

#change dir to script location
cd "${0%/*}"

TARGET="Release"
BASE=../..

REMOVE_WORKDIR=true

DATE=""
COMMIT=""

RPM_MIN_DIST="f23"

function help {
    echo "$0 {version-number} [-d] [-n] [-l]"
    echo
    echo -e "-d\tuse Debug configuration"
    echo -e "-n\tcreate a nightly build with date and commit SHA"
    echo -e "-l\tdo not remove workdir after building"
}

function is_dep_available {
    if ! command -v $1 >/dev/null 2>&1
    then
        echo "$1 is missing. Install it to continue."
        return 1
    fi
    return 0
}

if [ $# -lt 1 ]
then
    help
    exit
fi

if ! is_dep_available gem
then
    exit
fi

export PATH=`gem environment gemdir`/bin:$PATH

#expand this list if needed. bsdtar is required for arch packages.
if ! is_dep_available fpm ||\
    ! is_dep_available rpm ||\
    ! is_dep_available bsdtar
then
    exit
fi

VERSION=$1

shift
while getopts "dnl" opt
do
    case $opt in
        d)
            TARGET="Debug"
            ;;
        n)
            DATE="+`date +%Y%m%d`"
            COMMIT="git`git rev-parse --short HEAD`"
            ;;
        l)
            REMOVE_WORKDIR=false
            ;;
        \?)
            echo "Invalid option: -$OPTARG"
            help
            exit
            ;;
    esac
done

VERSION="$VERSION$DATE$COMMIT"

DIR=emul8_$VERSION

. common_copy_files.sh

PACKAGES=packages/$TARGET
OUTPUT=$BASE/$PACKAGES

GENERAL_FLAGS=(\
    -f -n emul8 -v $VERSION --license MIT\
    --category devel --provides emul8 -a native\
    -m 'Antmicro <emul8@antmicro.com>'\
    --vendor 'Antmicro <emul8@antmicro.com>'\
    --description 'The Emul8 Framework'\
    --url 'www.emul8.org'\
    --after-install linux/update_icon_cache.sh\
    --after-remove linux/update_icon_cache.sh\
    $DIR/=/opt/emul8\
    linux/emul8.sh=/usr/bin/emul8\
    linux/Emul8.desktop=/usr/share/applications/Emul8.desktop\
    linux/icons/=/usr/share/icons/hicolor
    )

### create debian package
fpm -s dir -t deb\
    -d 'mono-complete >= 4.6' -d gtk-sharp2 -d screen -d gksu\
    --deb-no-default-config-files\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/deb
deb=emul8*deb
echo -n "Created a Debian package in $PACKAGES/deb/"
echo $deb
mv $deb $OUTPUT/deb

### create rpm package
fpm -s dir -t rpm\
    -d 'mono-complete >= 4.6' -d gtk-sharp2 -d screen -d beesu\
    --rpm-dist $RPM_MIN_DIST\
    --rpm-auto-add-directories\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/rpm
rpm=emul8*rpm
echo -n "Created a Fedora package in $PACKAGES/rpm/"
echo $rpm
mv $rpm $OUTPUT/rpm

### create arch package
fpm -s dir -t pacman\
    -d mono -d gtk-sharp-2 -d screen -d gksu\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/arch
arch=emul*.pkg.tar.xz
echo -n "Created an Arch package in $PACKAGES/arch/"
echo $arch
mv $arch $OUTPUT/arch

#cleanup unless user requests otherwise
if $REMOVE_WORKDIR
then
    rm -rf $DIR
fi
