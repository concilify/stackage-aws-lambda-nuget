docker build examples -t example-lambdas

$id = docker create example-lambdas
docker cp ${id}:/app/Lambda.Basic.Example.zip .
docker cp ${id}:/app/Lambda.Middleware.Example.zip .
docker rm -v $id
