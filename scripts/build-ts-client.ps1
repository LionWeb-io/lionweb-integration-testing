# This PowerShell script builds the TS client (to be run by Node.js)
# by installing the appropriate NPM dependencies, and then calling the TS transpiler.
#
# It takes one optional argument which governs which version of the LionWeb TS packages are installed:
#   - if empty, then the version specified by the package.json (as it currently is) is used;
#   - if "local", then the LionWeb TS packages as they are on path ../lionweb-typescript/packages (relative to this repo’s root) is used'
#   - otherwise, the LionWeb TS packages as published on https://www.npmjs.com, in the specified version.

$LionWebTsVersion = $Args[0]

cd ts/

$LionWebTsPackages=@(
    "class-core-test-language",
    "core",
    "delta-protocol-client",
    "delta-protocol-common",
    "delta-protocol-low-level-client-ws",
    "delta-protocol-repository-ws",
    "json",
    "node-utils",
    "ts-utils",
    "utilities"
)

if ($LionWebTsVersion)
{
    if ($LionWebTsVersion -eq "local") {
        Write-Host "linking to the local LionWeb TS implementation"
        $deps = $LionWebTsPackages |% { "../../lionweb-typescript/packages/$_" }
        npm install @deps
    }
    elseif ($LionWebTsVersion -eq "specified")
    {
        Write-Host "installing the version of the LionWeb TS packages as specified in package.json (without modification)"
        npm install
    }
    else
    {
        Write-Host "installing the following version of the LionWeb TS packages: $LionWebTsVersion"
        $deps = $LionWebTsPackages |% { "@lionweb/$_@$LionWebTsVersion" }
        npm install @deps
    }
}
else
{
    Write-Error "no version for LionWeb TS packages provided: valid values are 'local', 'specified', and any published version"
    exit 2
}

npm run build:client

cd ..

