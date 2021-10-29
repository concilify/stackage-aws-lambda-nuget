docker build lambda -t lambda-build

$id = docker create lambda-build
docker cp ${id}:/lambda/Stackage.LambdaPackage.zip .
docker rm -v $id
