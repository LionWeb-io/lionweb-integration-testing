#!/bin/sh -e

# install AJV:
npm init -y
npm install ajv@8.17.1
npm install ajv-cli@5.0.0
npm install ajv-formats@3.0.1

sh download-serialization.schema.json.sh

# validate schema using AJV:
JAVA_LIONCORE_SERIALIZATION=../repos/lionweb-java/core/src/test/resources/serialization/lioncore.json
./node_modules/.bin/ajv -c ajv-formats --spec=draft2020 --strict=true --allErrors=true --allowUnionTypes=true test -s serialization.schema.json -d $JAVA_LIONCORE_SERIALIZATION --valid

