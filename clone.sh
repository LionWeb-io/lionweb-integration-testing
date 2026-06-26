#!/bin/sh -e

rm -rf ../repos
mkdir ../repos
git clone --depth 1 https://github.com/LionWeb-io/lionweb-jvm ../repos/lionweb-jvm
git clone --depth 1 https://github.com/LionWeb-io/specification ../repos/specification
git clone --depth 1 https://github.com/LionWeb-io/lionweb-mps ../repos/lionweb-mps
git clone --depth 1 https://github.com/LionWeb-io/lionweb-typescript ../repos/lionweb-typescript

