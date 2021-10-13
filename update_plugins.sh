#!/bin/bash
# Copyright 2021 The Aha001 Team.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# A simple script to manually update the SeedLang dll to the newest version.
#
# Pre-requisites
#
# NuGet client tools:
# https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools

# Sets up.
readonly TEMP_DIR="./Temp"
readonly TEMP_PLUGINS_DIR="${TEMP_DIR}/Plugins"
readonly PLUGINS_DIR="Assets/Plugins"
if [[ ! -d "${TEMP_DIR}" ]]; then
  mkdir "${TEMP_DIR}"
fi
if [[ ! -d "${TEMP_PLUGINS_DIR}" ]]; then
  mkdir "${TEMP_PLUGINS_DIR}"
fi

# Updates SeedLang.dll from NuGet. To keep this script simple, no dependecny
# checking is done for now.
readonly SEEDLANG_VERSION=`nuget list SeedLang -PreRelease | head -n 1`
echo "Updating: ${SEEDLANG_VERSION} ..."
readonly SEEDLANG_PATH="${SEEDLANG_VERSION/ //}"
readonly SEEDLANG_PACKAGE="${TEMP_PLUGINS_DIR}/SeedLang.nuget.zip"
curl -L "https://www.nuget.org/api/v2/package/${SEEDLANG_PATH}" \
  -o "${SEEDLANG_PACKAGE}"
unzip "${SEEDLANG_PACKAGE}" -d "${TEMP_PLUGINS_DIR}"
readonly SEEDLANG_DLL="${TEMP_PLUGINS_DIR}/lib/netstandard2.0/SeedLang.dll"
readonly SEEDLANG_VERSION_FILE="${PLUGINS_DIR}/SeedLang.version.txt"
chmod 640 "${SEEDLANG_DLL}"
cp "${SEEDLANG_DLL}" "${PLUGINS_DIR}"
echo "${SEEDLANG_VERSION}" > "${SEEDLANG_VERSION_FILE}"

# Clears temp files.
if [[ -d "${TEMP_PLUGINS_DIR}" ]]; then
  rm -rf "${TEMP_PLUGINS_DIR}"
fi
