docker build lambda -t package-name-lower

$id = docker create package-name-lower
docker cp ${id}:/lambda/Stackage.LambdaPackage.zip .
docker rm -v $id
