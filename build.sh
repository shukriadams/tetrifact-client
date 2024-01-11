set -e

TAG=$1

if [ -z "$TAG" ]; then
    echo "version required";
    exit 1
fi

# Write tag to json file, this will be picked up build.cake and copied into build folder after cake is done building
echo $TAG > ./src/currentversion.txt

# call cake build here, this builds and creates zip, check build.cake for details
cd src
dotnet restore
dotnet publish

# build installer
rm -rf installers
mkdir -p installers
#makensis //DPRODUCT_VERSION=$TAG ./build.nsi