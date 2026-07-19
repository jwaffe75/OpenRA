#!/bin/bash

# I couldn't find a good script or makefile command that would make a portable install directory for linux, so I made this.

targetPlatform='linux-x64'

# These functions copied from ./packaging/functions.sh

# Compile and publish the core engine and specified mod assemblies to the target directory
# Arguments:
#   SRC_PATH: Path to the root OpenRA directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
#   TARGETPLATFORM: Platform type (win-x64, osx-x64, osx-arm64, linux-x64, linux-arm64, unix-generic)
#   COPY_GENERIC_LAUNCHER: If set to True the OpenRA.exe will also be copied (True, False)
#   COPY_CNC_DLL: If set to True the OpenRA.Mods.Cnc.dll will also be copied (True, False)
#   COPY_D2K_DLL: If set to True the OpenRA.Mods.D2k.dll will also be copied (True, False)
# Used by:
#   Makefile (install target for local installs and downstream packaging)
#   Windows packaging
#   macOS packaging
#   Linux AppImage packaging
#   Mod SDK Windows packaging
#   Mod SDK macOS packaging
#   Mod SDK Linux AppImage packaging
install_assemblies() (
	set -o errexit || exit $?

	SRC_PATH="${1}"
	DEST_PATH="${2}"
	TARGETPLATFORM="${3}"
	COPY_GENERIC_LAUNCHER="${4}"
	COPY_CNC_DLL="${5}"
	COPY_D2K_DLL="${6}"

	ORIG_PWD=$(pwd)
	cd "${SRC_PATH}"

    dotnet publish -c Release -p:TargetPlatform="${TARGETPLATFORM}" -p:CopyGenericLauncher="${COPY_GENERIC_LAUNCHER}" -p:CopyCncDll="${COPY_CNC_DLL}" -p:CopyD2kDll="${COPY_D2K_DLL}" -r "${TARGETPLATFORM}" -p:PublishDir="${DEST_PATH}" --self-contained true
	cd "${ORIG_PWD}"
)

# Copy the core engine and specified mod data to the target directory
# Arguments:
#   SRC_PATH: Path to the root OpenRA directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
#   MOD [MOD...]: One or more mod ids to copy (cnc, d2k, ra)
# Used by:
#   Makefile (install target for local installs and downstream packaging)
#   Linux AppImage packaging
#   macOS packaging
#   Windows packaging
#   Mod SDK Linux AppImage packaging
#   Mod SDK macOS packaging
#   Mod SDK Windows packaging
install_data() (
	set -o errexit || exit $?

	SRC_PATH="${1}"
	DEST_PATH="${2}"
	shift 2

	"${SRC_PATH}"/fetch-geoip.sh

	echo "Installing engine files to ${DEST_PATH}"
	for FILE in VERSION AUTHORS COPYING IP2LOCATION-LITE-DB1.IPV6.BIN.ZIP "global mix database.dat"; do
		install -m644 "${SRC_PATH}/${FILE}" "${DEST_PATH}"
	done

	cp -r "${SRC_PATH}/glsl" "${DEST_PATH}"

	echo "Installing common mod files to ${DEST_PATH}"
	install -d "${DEST_PATH}/mods"
	cp -r "${SRC_PATH}/mods/common" "${DEST_PATH}/mods/"

	while [ -n "${1}" ]; do
		MOD_ID="${1}"
		if [ "${MOD_ID}" = "ra" ] || [ "${MOD_ID}" = "cnc" ] || [ "${MOD_ID}" = "d2k" ]; then
			echo "Installing mod ${MOD_ID} to ${DEST_PATH}"
			cp -r "${SRC_PATH}/mods/${MOD_ID}" "${DEST_PATH}/mods/"
			cp -r "${SRC_PATH}/mods/common-content" "${DEST_PATH}/mods/"
			cp -r "${SRC_PATH}/mods/${MOD_ID}-content" "${DEST_PATH}/mods/"
		fi

		shift
	done
)

if [ -e install ]
then
	rm -r install
fi

mkdir -p install
install_assemblies . install "$targetPlatform" True True True
install_data . install cnc d2k ra ts



# For some reason, even with the output directory set to 'install' from install_assemblies,
# it still doesn't install the binaries into the spot they need to be for a custom prefix
cp -r bin/* install
